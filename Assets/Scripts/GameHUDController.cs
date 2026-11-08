using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHUDController : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private GameController gameController;

    [Header("HUD")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text weightText;

    [Header("Game Ended Menu")]
    [SerializeField] private GameObject gameEndedPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject loseResultImage;
    [SerializeField] private GameObject winResultImage;

    private void Awake()
    {
        HideGameEndedMenu();
    }

    private void LateUpdate()
    {
        if (gameController == null)
        {
            return;
        }

        if (timeText != null)
        {
            timeText.text =
                $"Time: {Mathf.CeilToInt(gameController.RemainingTime)}";
        }

        if (weightText != null)
        {
            weightText.text =
                $"{gameController.CaughtWeight:0.#}" +
                $" / {gameController.TargetWeight:0.#} kg";
        }
    }

    public void DisplayGameEndedMenu()
    {
        if (gameController == null)
        {
            return;
        }

        bool won = gameController.HasWon;

        if (winResultImage != null)
        {
            winResultImage.SetActive(won);
        }

        if (loseResultImage != null)
        {
            loseResultImage.SetActive(!won);
        }

        // 해당 결과 그림이 없을 때만 글씨를 대신 표시해.
        bool hasResultImage = won
            ? winResultImage != null
            : loseResultImage != null;

        if (resultText != null)
        {
            resultText.text = won ? "You Win!" : "You Lose!";
            resultText.gameObject.SetActive(!hasResultImage);
        }

        if (gameEndedPanel != null)
        {
            gameEndedPanel.SetActive(true);
        }
    }

    public void HideGameEndedMenu()
    {
        if (winResultImage != null)
        {
            winResultImage.SetActive(false);
        }

        if (loseResultImage != null)
        {
            loseResultImage.SetActive(false);
        }

        if (gameEndedPanel != null)
        {
            gameEndedPanel.SetActive(false);
        }
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}