using UnityEngine;

public class ClockTimerUI : MonoBehaviour
{
    [SerializeField] private GameController controller;
    [SerializeField] private RectTransform clockHand;

    private float startingTime;
    private bool initialized;

    private void LateUpdate()
    {
        if (controller == null || clockHand == null)
        {
            return;
        }

        float remainingTime =
            Mathf.Max(0f, controller.RemainingTime);

        if (!initialized)
        {
            if (remainingTime <= 0f)
            {
                return;
            }

            startingTime = remainingTime;
            initialized = true;
        }

        float progress = Mathf.Clamp01(
            1f - remainingTime / startingTime
        );

        clockHand.localRotation = Quaternion.Euler(
            0f,
            0f,
            -360f * progress
        );
    }
}