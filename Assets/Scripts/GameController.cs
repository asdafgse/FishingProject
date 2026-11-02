using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float targetWeight = 10f;

    [Header("References")]
    [SerializeField] private GameHUDController hud;

    public float TimeLimit => timeLimit;
    public float TargetWeight => targetWeight;
    public GameHUDController HUD => hud;

    public float RemainingTime { get; set; }
    public float CaughtWeight { get; set; }
    public bool HasWon { get; set; }

    public void ResetGameData()
    {
        RemainingTime = timeLimit;
        CaughtWeight = 0f;
        HasWon = false;
    }

    public void TestCatchFish()
    {
        GameFSM fsm = GetComponent<GameFSM>();

        if (fsm == null ||
            fsm.CurrentState != fsm.PlayState ||
            RemainingTime <= 0f)
        {
            return;
        }

        CaughtWeight += 2f;
    }
}