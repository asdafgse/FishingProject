using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialGuide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;
    [SerializeField] private Slider chargeSlider;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.4f;

    private CanvasGroup canvasGroup;
    private bool chargingStarted;
    private bool fading;
    private float fadeTimer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        foreach (Graphic graphic in
            GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }
    }

    private void Update()
    {
        if (gameFSM == null || chargeSlider == null)
            return;

        if (gameFSM.CurrentState != gameFSM.PlayState)
            return;

        if (!fading)
        {
            if (chargeSlider.gameObject.activeInHierarchy)
            {
                chargingStarted = true;
            }
            else if (chargingStarted)
            {
                // 차징 후 게이지가 꺼지면 첫 캐스팅 완료
                fading = true;
                fadeTimer = 0f;
            }
        }

        if (!fading)
            return;

        fadeTimer += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(
            fadeTimer / Mathf.Max(0.01f, fadeDuration)
        );

        canvasGroup.alpha = 1f - progress;

        if (progress >= 1f)
            gameObject.SetActive(false);
    }
}