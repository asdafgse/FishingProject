using UnityEngine;

public class WeightBoardPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;

    [Header("Animation")]
    [SerializeField] private float scaleMultiplier = 1.12f;
    [SerializeField] private float duration = 0.3f;

    private Vector3 originalScale;
    private float previousWeight;
    private float timer;
    private bool pulsing;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;
        pulsing = false;

        if (gameController != null)
            previousWeight = gameController.CaughtWeight;
    }

    private void Update()
    {
        if (gameController == null)
            return;

        float currentWeight = gameController.CaughtWeight;

        if (currentWeight > previousWeight)
        {
            timer = 0f;
            pulsing = true;
        }

        previousWeight = currentWeight;

        if (!pulsing)
            return;

        timer += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(
            timer / Mathf.Max(0.01f, duration)
        );

        float pulse = Mathf.Sin(progress * Mathf.PI);

        float scale = Mathf.Lerp(
            1f,
            scaleMultiplier,
            pulse
        );

        transform.localScale = originalScale * scale;

        if (progress >= 1f)
        {
            transform.localScale = originalScale;
            pulsing = false;
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        pulsing = false;
    }
}