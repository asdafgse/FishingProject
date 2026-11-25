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

    [Header("Game Ended Audio")]
    [SerializeField] private AudioSource gameBGM;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [SerializeField, Range(0f, 1f)]
    private float resultVolume = 0.8f;

    private AudioSource resultAudioSource;
    private bool resultSoundPlayed;

    private void Awake()
    {
        resultAudioSource =
            gameObject.AddComponent<AudioSource>();

        resultAudioSource.playOnAwake = false;
        resultAudioSource.loop = false;
        resultAudioSource.spatialBlend = 0f;
        resultAudioSource.volume = 1f;

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

        if (gameBGM != null)
        {
            gameBGM.Stop();
        }

        // 여러 번 호출되어도 효과음은 한 번만 재생
        if (!resultSoundPlayed)
        {
            resultSoundPlayed = true;

            AudioClip resultSound = won
                ? winSound
                : loseSound;

            if (resultSound != null)
            {
                resultAudioSource.PlayOneShot(
                    resultSound,
                    resultVolume
                );
            }
        }
    }

    public void HideGameEndedMenu()
    {
        resultSoundPlayed = false;

        if (resultAudioSource != null)
        {
            resultAudioSource.Stop();
        }

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