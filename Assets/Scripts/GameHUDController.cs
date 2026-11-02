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
                $"Weight: {gameController.CaughtWeight:0.#}" +
                $" / {gameController.TargetWeight:0.#} kg";
        }
    }

    public void DisplayGameEndedMenu()
    {
        if (resultText != null && gameController != null)
        {
            resultText.text = gameController.HasWon
                ? "You Win!"
                : "You Lose!";
        }

        if (gameEndedPanel != null)
        {
            gameEndedPanel.SetActive(true);
        }
    }

    public void HideGameEndedMenu()
    {
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