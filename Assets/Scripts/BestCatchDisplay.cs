using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class BestCatchDisplay : MonoBehaviour
{
    private TMP_Text recordText;

    private void Awake()
    {
        recordText = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (SaveManager.Instance == null ||
            SaveManager.Instance.Data == null)
        {
            recordText.text = "Best Catch: 0 kg";
            return;
        }

        float weight =
            SaveManager.Instance.Data.bestCatchWeight;

        recordText.text = $"Best Catch: {weight:0.#} kg";
    }
}