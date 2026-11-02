using UnityEngine;

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

    [Header("Fishing Settings")]
    [SerializeField] private float minCastSpeed = 4f;
    [SerializeField] private float maxCastSpeed = 12f;
    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float gravity = 9f;
    [SerializeField] private float reelSpeed = 6f;
    [SerializeField] private float bottomY = -3.5f;

    private enum HookState
    {
        Ready,
        Charging,
        InWater
    }

    private HookState hookState;
    private LineRenderer fishingLine;
    private Material lineMaterial;

    private Vector3 velocity;
    private float charge;
    private bool wasHeld;

    private void Awake()
    {
        fishingLine = GetComponent<LineRenderer>();

        fishingLine.useWorldSpace = true;
        fishingLine.loop = false;
        fishingLine.positionCount = 2;
        fishingLine.startWidth = 0.025f;
        fishingLine.endWidth = 0.025f;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        if (shader != null)
        {
            lineMaterial = new Material(shader);
            lineMaterial.SetColor("_BaseColor", Color.white);
            fishingLine.material = lineMaterial;
        }
    }

    private void Start()
    {
        if (rodTip == null ||
            gameFSM == null ||
            gameCamera == null)
        {
            Debug.LogError(
                "FishingController의 References를 연결해줘."
            );

            enabled = false;
            return;
        }

        ResetHook();
    }

    private void Update()
    {
        bool held = IsPointerHeld();
        bool pressed = held && !wasHeld;
        bool released = !held && wasHeld;

        wasHeld = held;

        if (gameFSM.CurrentState != gameFSM.PlayState)
        {
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
                charge += Time.deltaTime;

                if (released)
                {
                    CastHook();
                }
                break;

            case HookState.InWater:
                if (held)
                {
                    ReelHook();
                }
                else
                {
                    SinkHook();
                }
                break;
        }
    }

    private void LateUpdate()
    {
        if (rodTip == null)
        {
            return;
        }

        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, transform.position);
    }

    private void CastHook()
    {
        Ray ray = gameCamera.ScreenPointToRay(
            GetPointerPosition()
        );

        Plane fishingPlane = new Plane(
            Vector3.forward,
            rodTip.position
        );

        Vector3 direction = new Vector3(1f, 0.5f, 0f);

        if (fishingPlane.Raycast(ray, out float distance))
        {
            direction = ray.GetPoint(distance) - rodTip.position;
        }

        // 우측으로 던지고, 위아래 각도를 제한한다.
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

        float speed = Mathf.Lerp(
            minCastSpeed,
            maxCastSpeed,
            power
        );

        velocity = direction * speed;
        hookState = HookState.InWater;
    }

    private void SinkHook()
    {
        velocity.y -= gravity * Time.deltaTime;

        Vector3 nextPosition =
            transform.position + velocity * Time.deltaTime;

        // 임시 낚시 영역 안에 머물도록 제한한다.
        if (nextPosition.x >= 8f)
        {
            nextPosition.x = 8f;
            velocity.x = 0f;
        }

        if (nextPosition.y <= bottomY)
        {
            nextPosition.y = bottomY;
            velocity = Vector3.zero;
        }

        nextPosition.z = rodTip.position.z;
        transform.position = nextPosition;
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
            rodTip.position) < 0.05f)
        {
            ResetHook();
        }
    }

    private void ResetHook()
    {
        transform.position = rodTip.position;
        velocity = Vector3.zero;
        charge = 0f;
        hookState = HookState.Ready;
    }

    private bool IsPointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null &&
               Pointer.current.press.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private Vector2 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}