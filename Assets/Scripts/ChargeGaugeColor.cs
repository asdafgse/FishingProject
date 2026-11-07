using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ChargeGaugeColor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField]
    private Color lowColor =
        new Color(0.2f, 0.9f, 0.3f);

    [SerializeField]
    private Color middleColor =
        new Color(1f, 0.85f, 0.1f);

    [SerializeField]
    private Color fullColor =
        new Color(1f, 0.15f, 0.1f);

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // 연결하지 않았으면 Slider의 채움 이미지 찾기
        if (fillImage == null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        UpdateColor();
    }

    private void LateUpdate()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (slider == null || fillImage == null)
            return;

        float power = slider.normalizedValue;

        if (power <= 0.5f)
        {
            fillImage.color = Color.Lerp(
                lowColor,
                middleColor,
                power * 2f
            );
        }
        else
        {
            fillImage.color = Color.Lerp(
                middleColor,
                fullColor,
                (power - 0.5f) * 2f
            );
        }
    }
}