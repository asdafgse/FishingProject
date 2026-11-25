using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(LineRenderer))]
public class FishingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private GameFSM gameFSM;
    [SerializeField] private Camera gameCamera;

    [Header("Charge UI")]
    [SerializeField] private Slider chargeSlider;

    [Header("Fishing Settings")]
    [SerializeField] private float minCastSpeed = 4f;
    [SerializeField] private float maxCastSpeed = 12f;
    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float gravity = 9f;
    [SerializeField] private float reelSpeed = 6f;
    [SerializeField] private float bottomY = -3.5f;

    [Header("Catch Settings")]
    [SerializeField] private float catchRadius = 0.45f;

    [Header("Water Splash")]
    [SerializeField] private ParticleSystem waterSplashPrefab;
    [SerializeField] private float waterSurfaceY = 0f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip reelSound;
    [SerializeField] private AudioClip catchSound;

    [SerializeField, Range(0f, 1f)]
    private float castVolume = 0.7f;

    [SerializeField, Range(0f, 1f)]
    private float reelVolume = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float catchVolume = 0.8f;

    private enum HookState
    {
        Ready,
        Charging,
        InWater
    }

    private HookState hookState;
    private LineRenderer fishingLine;
    private Material lineMaterial;
    private GameController controller;
    private FishController caughtFish;

    private AudioSource effectSource;
    private AudioSource reelSource;

    private Vector3 velocity;
    private Vector2 lastPointerPosition;

    private float charge;
    private bool wasHeld;
    private bool splashPlayed;

    private void Awake()
    {
        fishingLine = GetComponent<LineRenderer>();
        fishingLine.useWorldSpace = true;
        fishingLine.loop = false;
        fishingLine.positionCount = 2;
        fishingLine.startWidth = 0.025f;
        fishingLine.endWidth = 0.025f;
        fishingLine.startColor = Color.white;
        fishingLine.endColor = Color.white;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        if (shader != null)
        {
            lineMaterial = new Material(shader);
            lineMaterial.SetColor("_BaseColor", Color.white);
            fishingLine.material = lineMaterial;
        }

        // 던지기와 획득 소리
        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.playOnAwake = false;
        effectSource.loop = false;
        effectSource.spatialBlend = 0f;
        effectSource.volume = 1f;

        // 끌어오기 소리는 별도로 반복 재생
        reelSource = gameObject.AddComponent<AudioSource>();
        reelSource.playOnAwake = false;
        reelSource.loop = true;
        reelSource.spatialBlend = 0f;

        if (chargeSlider != null)
        {
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
            chargeSlider.wholeNumbers = false;
            chargeSlider.interactable = false;

            foreach (Graphic graphic in
                chargeSlider.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            chargeSlider.SetValueWithoutNotify(0f);
            chargeSlider.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (rodTip == null ||
            gameFSM == null ||
            gameCamera == null)
        {
            Debug.LogError(
                "Rod Tip, Game FSM, Game Camera를 연결해줘."
            );

            enabled = false;
            return;
        }

        controller = gameFSM.GetComponent<GameController>();

        if (controller == null)
        {
            Debug.LogError("GameController가 필요해.");
            enabled = false;
            return;
        }

        ResetHook();
    }

    private void Update()
    {
        ReadPointer(out bool held, out Vector2 pointerPosition);

        bool pressed = held && !wasHeld;
        bool released = !held && wasHeld;

        if (held)
            lastPointerPosition = pointerPosition;

        wasHeld = held;

        if (gameFSM.CurrentState != gameFSM.PlayState ||
            controller.RemainingTime <= 0f)
        {
            StopReelSound();

            if (hookState == HookState.Charging)
            {
                charge = 0f;
                hookState = HookState.Ready;
            }

            UpdateChargeUI(false);
            return;
        }

        switch (hookState)
        {
            case HookState.Ready:
                transform.position = rodTip.position;

                if (pressed)
                {
                    charge = 0f;
                    hookState = HookState.Charging;
                }
                break;

            case HookState.Charging:
                transform.position = rodTip.position;

                charge = Mathf.Min(
                    charge + Time.deltaTime,
                    Mathf.Max(0.01f, chargeTime)
                );

                if (released)
                    CastHook();

                break;

            case HookState.InWater:
                CheckForFish();

                if (held)
                    ReelHook();
                else
                    SinkHook();

                if (hookState == HookState.InWater)
                    CheckForFish();

                break;
        }

        UpdateReelSound(
            held &&
            hookState == HookState.InWater &&
            gameFSM.CurrentState == gameFSM.PlayState &&
            controller.RemainingTime > 0f
        );

        UpdateChargeUI(
            hookState == HookState.Charging
        );
    }

    private void LateUpdate()
    {
        if (rodTip == null)
            return;

        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, transform.position);
    }

    private void UpdateChargeUI(bool visible)
    {
        if (chargeSlider == null)
            return;

        if (chargeSlider.gameObject.activeSelf != visible)
            chargeSlider.gameObject.SetActive(visible);

        float power = visible
            ? Mathf.Clamp01(
                charge / Mathf.Max(0.01f, chargeTime)
            )
            : 0f;

        chargeSlider.SetValueWithoutNotify(power);
    }

    private void CastHook()
    {
        Ray ray = gameCamera.ScreenPointToRay(
            lastPointerPosition
        );

        Plane plane = new Plane(
            Vector3.forward,
            rodTip.position
        );

        Vector3 direction = new Vector3(1f, 0.5f, 0f);

        if (plane.Raycast(ray, out float distance))
        {
            direction =
                ray.GetPoint(distance) - rodTip.position;
        }

        float angle = Mathf.Atan2(
            direction.y,
            Mathf.Max(0.1f, direction.x)
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -30f, 70f);

        direction = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f
        );

        float power = Mathf.Clamp01(
            charge / Mathf.Max(0.01f, chargeTime)
        );

        velocity = direction * Mathf.Lerp(
            minCastSpeed,
            maxCastSpeed,
            power
        );

        splashPlayed = false;
        hookState = HookState.InWater;

        PlayEffect(castSound, castVolume);
    }

    private void SinkHook()
    {
        Vector3 previous = transform.position;

        velocity.y -= gravity * Time.deltaTime;

        Vector3 next =
            previous + velocity * Time.deltaTime;

        if (next.x >= 8f)
        {
            next.x = 8f;
            velocity.x = 0f;
        }

        if (next.y <= bottomY)
        {
            next.y = bottomY;
            velocity = Vector3.zero;
        }

        next.z = rodTip.position.z;

        CheckWaterEntry(previous, next);
        transform.position = next;
    }

    private void CheckWaterEntry(
        Vector3 previous,
        Vector3 next
    )
    {
        if (splashPlayed)
            return;

        if (previous.y <= waterSurfaceY ||
            next.y > waterSurfaceY)
        {
            return;
        }

        splashPlayed = true;

        if (waterSplashPrefab == null)
            return;

        float fraction =
            (previous.y - waterSurfaceY) /
            (previous.y - next.y);

        Vector3 splashPosition = Vector3.Lerp(
            previous,
            next,
            fraction
        );

        splashPosition.y = waterSurfaceY;

        ParticleSystem splash = Instantiate(
            waterSplashPrefab,
            splashPosition,
            waterSplashPrefab.transform.rotation
        );

        var main = splash.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;

        splash.Play(true);
    }

    private void CheckForFish()
    {
        if (caughtFish != null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            catchRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            FishController fish =
                hit.GetComponentInParent<FishController>();

            if (fish != null && fish.TryHook(transform))
            {
                caughtFish = fish;

                Debug.Log(
                    $"물고기가 걸렸어! {fish.Weight} kg"
                );

                break;
            }
        }
    }

    private void ReelHook()
    {
        velocity = Vector3.zero;

        transform.position = Vector3.MoveTowards(
            transform.position,
            rodTip.position,
            reelSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
                transform.position,
                rodTip.position
            ) < 0.05f)
        {
            StopReelSound();

            if (caughtFish != null &&
                caughtFish.isActiveAndEnabled)
            {
                controller.CaughtWeight += caughtFish.Weight;

                PlayEffect(catchSound, catchVolume);

                Debug.Log(
                    $"잡았다! 총 {controller.CaughtWeight} kg"
                );

                caughtFish.LandFish();
            }

            caughtFish = null;
            ResetHook();
        }
    }

    private void ResetHook()
    {
        StopReelSound();

        transform.position = rodTip.position;
        velocity = Vector3.zero;
        charge = 0f;
        splashPlayed = false;
        hookState = HookState.Ready;

        UpdateChargeUI(false);
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip == null || effectSource == null)
            return;

        effectSource.PlayOneShot(clip, volume);
    }

    private void UpdateReelSound(bool shouldPlay)
    {
        if (reelSource == null)
            return;

        if (!shouldPlay || reelSound == null)
        {
            StopReelSound();
            return;
        }

        reelSource.volume = reelVolume;

        if (reelSource.clip != reelSound)
        {
            reelSource.Stop();
            reelSource.clip = reelSound;
        }

        if (!reelSource.isPlaying)
            reelSource.Play();
    }

    private void StopReelSound()
    {
        if (reelSource != null && reelSource.isPlaying)
            reelSource.Stop();
    }

    private void ReadPointer(
        out bool held,
        out Vector2 position
    )
    {
        held = false;
        position = lastPointerPosition;

#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.isPressed)
            {
                held = true;
                position = touch.position.ReadValue();
                return;
            }
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed)
        {
            held = true;
            position = Mouse.current.position.ReadValue();
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            held =
                touch.phase != TouchPhase.Ended &&
                touch.phase != TouchPhase.Canceled;

            position = touch.position;
            return;
        }

        held = Input.GetMouseButton(0);
        position = Input.mousePosition;
#endif
    }

    private void OnDisable()
    {
        StopReelSound();
        wasHeld = false;

        if (hookState == HookState.Charging)
        {
            charge = 0f;
            hookState = HookState.Ready;
        }

        UpdateChargeUI(false);
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
            Destroy(lineMaterial);
    }
}