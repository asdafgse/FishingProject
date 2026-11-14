using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MenuEnterAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float startScale = 0.8f;

    private CanvasGroup canvasGroup;
    private RectTransform[] items;
    private Vector3[] originalScales;
    private Coroutine animationRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 패널 바로 아래의 표지판과 버튼을 기록
        int count = transform.childCount;

        items = new RectTransform[count];
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            items[i] = transform.GetChild(i)
                .GetComponent<RectTransform>();

            if (items[i] != null)
                originalScales[i] = items[i].localScale;
        }
    }

    private void OnEnable()
    {
        animationRoutine = StartCoroutine(AnimateEnter());
    }

    private IEnumerator AnimateEnter()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        SetScale(startScale);

        float elapsed = 0f;
        float animationDuration = Mathf.Max(0.01f, duration);

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / animationDuration
            );

            // 끝으로 갈수록 부드럽게 감속
            float eased =
                1f - Mathf.Pow(1f - progress, 3f);

            canvasGroup.alpha = eased;

            SetScale(Mathf.Lerp(
                startScale,
                1f,
                eased
            ));

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;

        SetScale(1f);
        animationRoutine = null;
    }

    private void SetScale(float multiplier)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                items[i].localScale =
                    originalScales[i] * multiplier;
            }
        }
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        if (items != null)
            SetScale(1f);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
    }
}