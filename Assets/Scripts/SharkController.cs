using UnityEngine;

public class SharkController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip eatClip;

    [Header("Animation State Names")]
    [SerializeField] private string swimState = "Shark_Swim";
    [SerializeField] private string eatState = "Shark_Eat";

    [Header("Sprite Direction")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float turnSpeed = 3f;

    [Header("Direction Stability")]
    [Tooltip("이 거리 안에서는 좌우 방향을 바꾸지 않음")]
    [SerializeField] private float flipDeadZone = 0.6f;

    [SerializeField] private float flipCooldown = 0.8f;

    [Header("Swimming Area")]
    [SerializeField] private float leftLimit = -3f;
    [SerializeField] private float rightLimit = 7f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = -0.7f;

    [Header("Hunting")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float eatRadius = 0.5f;
    [SerializeField] private bool canEatHookedFish = true;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float eatDuration = 2.5f;
    [SerializeField] private float eatCooldown = 8f;

    [Header("Mouth Position")]
    [SerializeField]
    private Vector3 mouthOffset =
        new Vector3(0.7f, 0f, 0f);

    private FishController targetFish;
    private Vector3 originalScale;
    private Vector3 patrolTarget;

    private float cooldownTimer;
    private float searchTimer;
    private float eatTimer;
    private float biteTime;
    private float facing = 1f;
    private float flipTimer;

    private bool isEating;
    private bool hasBitten;

    private int swimHash;
    private int eatHash;

    private void Start()
    {
        originalScale = transform.localScale;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (gameFSM == null ||
            animator == null ||
            eatClip == null)
        {
            Debug.LogError(
                "Shark의 Game FSM, Animator, Eat Clip을 연결해줘.",
                this
            );

            enabled = false;
            return;
        }

        swimHash = Animator.StringToHash(
            "Base Layer." + swimState
        );

        eatHash = Animator.StringToHash(
            "Base Layer." + eatState
        );

        if (!animator.HasState(0, swimHash) ||
            !animator.HasState(0, eatHash))
        {
            Debug.LogError(
                "Animator의 Base Layer에 " +
                "Shark_Swim, Shark_Eat 상태가 필요해.",
                this
            );

            enabled = false;
            return;
        }

        eatDuration = Mathf.Max(0.2f, eatDuration);

        float clipLength =
            Mathf.Max(0.01f, eatClip.length);

        float frameTime =
            1f / Mathf.Max(1f, eatClip.frameRate);

        biteTime = eatDuration * Mathf.Clamp01(
            (clipLength - frameTime) / clipLength
        );

        animator.applyRootMotion = false;
        cooldownTimer = initialDelay;

        ApplyFacing();
        SetPosition(transform.position);
        ChoosePatrolTarget();

        animator.Play(swimHash, 0, 0f);
    }

    private void Update()
    {
        bool playing =
            gameFSM.CurrentState == gameFSM.PlayState;

        if (!playing)
        {
            animator.speed = 0f;
            return;
        }

        animator.speed = isEating
            ? Mathf.Max(0.01f, eatClip.length) / eatDuration
            : 1f;

        float dt = Time.deltaTime;

        flipTimer = Mathf.Max(0f, flipTimer - dt);

        if (isEating)
        {
            UpdateEating(dt);
            return;
        }

        cooldownTimer = Mathf.Max(
            0f, cooldownTimer - dt
        );

        if (cooldownTimer > 0f)
        {
            Patrol(dt);
            return;
        }

        if (!CanTarget(targetFish))
            targetFish = null;

        searchTimer -= dt;

        if (targetFish == null && searchTimer <= 0f)
        {
            FindNearestFish();
            searchTimer = 0.3f;
        }

        if (targetFish == null)
        {
            Patrol(dt);
            return;
        }

        Vector3 targetPosition =
            targetFish.transform.position;

        // 입 위치가 아닌 몸 중심을 기준으로 방향 판단
        UpdateFacing(targetPosition, dt);

        MoveMouthToward(
            targetPosition,
            chaseSpeed,
            dt
        );

        if (Vector3.Distance(
                GetMouthPosition(),
                targetFish.transform.position
            ) <= eatRadius)
        {
            BeginEating();
        }
    }

    private bool CanTarget(FishController fish)
    {
        if (fish == null || !fish.isActiveAndEnabled)
            return false;

        if (!canEatHookedFish && fish.IsHooked)
            return false;

        Vector3 p = fish.transform.position;

        return p.x >= leftLimit &&
               p.x <= rightLimit &&
               p.y >= minY &&
               p.y <= maxY;
    }

    private void FindNearestFish()
    {
        FishController[] fishes =
            FindObjectsByType<FishController>(
                FindObjectsSortMode.None
            );

        float nearest =
            detectionRadius * detectionRadius;

        targetFish = null;

        foreach (FishController fish in fishes)
        {
            if (!CanTarget(fish))
                continue;

            float distance =
                (fish.transform.position -
                 transform.position).sqrMagnitude;

            if (distance < nearest)
            {
                nearest = distance;
                targetFish = fish;
            }
        }
    }

    private void BeginEating()
    {
        isEating = true;
        hasBitten = false;
        eatTimer = 0f;

        animator.speed =
            Mathf.Max(0.01f, eatClip.length) / eatDuration;

        animator.Play(eatHash, 0, 0f);
    }

    private void UpdateEating(float dt)
    {
        eatTimer += dt;

        // 먹는 동안에는 좌우 방향과 회전을 고정
        if (!hasBitten && CanTarget(targetFish))
        {
            MoveMouthToward(
                targetFish.transform.position,
                chaseSpeed,
                dt
            );
        }

        if (!hasBitten && eatTimer >= biteTime)
        {
            hasBitten = true;
            TryEatFish();
        }

        if (eatTimer >= eatDuration)
        {
            isEating = false;
            targetFish = null;
            cooldownTimer = eatCooldown;
            flipTimer = flipCooldown;

            animator.speed = 1f;
            animator.Play(swimHash, 0, 0f);

            ChoosePatrolTarget();
        }
    }

    private void TryEatFish()
    {
        if (!CanTarget(targetFish))
            return;

        if (Vector3.Distance(
                GetMouthPosition(),
                targetFish.transform.position
            ) > eatRadius)
        {
            return;
        }

        FishController fish = targetFish;
        targetFish = null;

        fish.gameObject.SetActive(false);
        Destroy(fish.gameObject);
    }

    private Vector3 GetMouthPosition()
    {
        float sourceDirection =
            spriteFacesRight ? 1f : -1f;

        Vector3 offset = mouthOffset;
        offset.x =
            Mathf.Abs(offset.x) * sourceDirection;

        return transform.TransformPoint(offset);
    }

    private void MoveMouthToward(
        Vector3 destination,
        float speed,
        float dt
    )
    {
        Vector3 difference =
            destination - GetMouthPosition();

        difference.z = 0f;

        SetPosition(
            transform.position +
            Vector3.ClampMagnitude(
                difference,
                speed * dt
            )
        );
    }

    private void Patrol(float dt)
    {
        if (Vector3.Distance(
                transform.position,
                patrolTarget
            ) < 0.2f)
        {
            ChoosePatrolTarget();
        }

        UpdateFacing(patrolTarget, dt);

        SetPosition(Vector3.MoveTowards(
            transform.position,
            patrolTarget,
            patrolSpeed * dt
        ));
    }

    private void ChoosePatrolTarget()
    {
        // 현재 위치에서 충분히 떨어진 지점 선택
        for (int i = 0; i < 10; i++)
        {
            patrolTarget = new Vector3(
                Random.Range(leftLimit, rightLimit),
                Random.Range(minY, maxY),
                0f
            );

            if (Vector3.Distance(
                    transform.position,
                    patrolTarget
                ) >= 1.5f)
            {
                break;
            }
        }
    }

    private void SetPosition(Vector3 p)
    {
        p.x = Mathf.Clamp(p.x, leftLimit, rightLimit);
        p.y = Mathf.Clamp(p.y, minY, maxY);
        p.z = 0f;

        transform.position = p;
    }

    private void UpdateFacing(Vector3 destination, float dt)
    {
        Vector3 direction =
            destination - transform.position;

        // 가까운 목표를 따라갈 때 좌우 반전 방지
        if (Mathf.Abs(direction.x) > flipDeadZone &&
            flipTimer <= 0f)
        {
            float desiredFacing =
                Mathf.Sign(direction.x);

            if (desiredFacing != facing)
            {
                facing = desiredFacing;
                flipTimer = flipCooldown;
            }
        }

        ApplyFacing();

        // 기울기도 작게 제한해서 흔들림 감소
        float tilt = 0f;

        if (Mathf.Abs(direction.x) > flipDeadZone)
        {
            tilt = Mathf.Atan2(
                direction.y,
                Mathf.Abs(direction.x)
            ) * Mathf.Rad2Deg * facing;

            tilt = Mathf.Clamp(tilt, -8f, 8f);
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0f, 0f, tilt),
            1f - Mathf.Exp(-turnSpeed * dt)
        );
    }

    private void ApplyFacing()
    {
        float sourceDirection =
            spriteFacesRight ? 1f : -1f;

        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) *
            facing * sourceDirection,
            originalScale.y,
            originalScale.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            GetMouthPosition(),
            eatRadius
        );
    }
}