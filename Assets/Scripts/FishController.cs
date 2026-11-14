using UnityEngine;

public class FishController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;

    [Header("Fish Settings")]
    [SerializeField] private float weight = 2f;
    [SerializeField] private float swimSpeed = 1.5f;
    [SerializeField] private float leftLimit = -3f;
    [SerializeField] private float rightLimit = 7f;

    [Header("Swimming Area")]
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = -0.7f;

    [Header("Natural Movement")]
    [SerializeField] private float turnSmoothness = 2f;
    [SerializeField] private float verticalSpeed = 0.5f;

    [Header("Mouth Position")]
    [SerializeField]
    private Vector3 mouthOffset =
        new Vector3(0.5f, 0f, 0f);

    [Header("Hooked Movement")]
    [SerializeField] private float hookedTurnSpeed = 8f;
    [SerializeField] private float struggleAngle = 8f;
    [SerializeField] private float struggleSpeed = 12f;

    public float Weight => weight;
    public bool IsHooked => attachedHook != null;

    private Transform attachedHook;
    private Vector3 originalScale;
    private Vector2 velocity;

    private float direction;
    private float targetSpeed;
    private float behaviorTimer;
    private float noiseSeed;
    private float swimTime;

    public void SetGameFSM(GameFSM value)
    {
        gameFSM = value;
    }

    private void Start()
    {
        originalScale = transform.localScale;

        transform.position = new Vector3(
            Random.Range(leftLimit, rightLimit),
            Random.Range(minY, maxY),
            0f
        );

        direction = Random.value < 0.5f ? -1f : 1f;
        noiseSeed = Random.Range(0f, 1000f);
        swimTime = Random.Range(0f, 100f);

        ChooseBehavior();

        velocity = new Vector2(
            direction * targetSpeed,
            0f
        );
    }

    private void Update()
    {
        if (gameFSM == null)
            return;

        if (gameFSM.CurrentState != gameFSM.PlayState)
            return;

        if (IsHooked)
            return;

        float dt = Time.deltaTime;

        swimTime += dt;
        behaviorTimer -= dt;

        if (behaviorTimer <= 0f)
            ChooseBehavior();

        if (transform.position.x > rightLimit - 0.8f)
            direction = -1f;
        else if (transform.position.x < leftLimit + 0.8f)
            direction = 1f;

        float noise = Mathf.PerlinNoise(
            noiseSeed,
            swimTime * 0.45f
        );

        float desiredY =
            (noise * 2f - 1f) * verticalSpeed;

        if (transform.position.y > maxY - 0.3f)
            desiredY = -verticalSpeed;
        else if (transform.position.y < minY + 0.3f)
            desiredY = verticalSpeed;

        Vector2 desiredVelocity = new Vector2(
            direction * targetSpeed,
            desiredY
        );

        float blend =
            1f - Mathf.Exp(-turnSmoothness * dt);

        velocity = Vector2.Lerp(
            velocity,
            desiredVelocity,
            blend
        );

        Vector3 nextPosition = transform.position;

        nextPosition.x += velocity.x * dt;
        nextPosition.y += velocity.y * dt;

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            leftLimit,
            rightLimit
        );

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            minY,
            maxY
        );

        nextPosition.z = 0f;
        transform.position = nextPosition;

        UpdateFacing(dt);
    }

    private void LateUpdate()
    {
        if (!IsHooked)
            return;

        bool isPlaying =
            gameFSM != null &&
            gameFSM.CurrentState == gameFSM.PlayState;

        if (isPlaying)
        {
            float dt = Time.deltaTime;

            float facing =
                transform.localScale.x >= 0f ? 1f : -1f;

            float wiggle = Mathf.Sin(
                Time.time * struggleSpeed + noiseSeed
            ) * struggleAngle;

            float targetAngle =
                90f * facing + wiggle;

            Quaternion targetRotation =
                Quaternion.Euler(0f, 0f, targetAngle);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-hookedTurnSpeed * dt)
            );
        }

        transform.position =
            attachedHook.position -
            transform.TransformVector(mouthOffset);
    }

    private void ChooseBehavior()
    {
        behaviorTimer = Random.Range(1.5f, 4f);

        targetSpeed =
            swimSpeed * Random.Range(0.5f, 1.4f);

        if (Random.value < 0.2f)
            direction *= -1f;
    }

    private void UpdateFacing(float dt)
    {
        if (Mathf.Abs(velocity.x) < 0.1f)
            return;

        float facingSign = Mathf.Sign(velocity.x);

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * facingSign,
            originalScale.y,
            originalScale.z
        );

        float tilt = Mathf.Atan2(
            velocity.y,
            Mathf.Abs(velocity.x)
        ) * Mathf.Rad2Deg * facingSign;

        tilt = Mathf.Clamp(tilt, -25f, 25f);

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, tilt);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1f - Mathf.Exp(-5f * dt)
        );
    }

    public bool TryHook(Transform hook)
    {
        if (!isActiveAndEnabled || IsHooked || hook == null)
            return false;

        attachedHook = hook;
        velocity = Vector2.zero;

        return true;
    }

    public void LandFish()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}