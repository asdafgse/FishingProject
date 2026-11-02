using UnityEngine;

[RequireComponent(typeof(GameController))]
public class GameFSM : StateMachineMB
{
    public GameSetupState SetupState { get; private set; }
    public GamePlayState PlayState { get; private set; }
    public GameEndedState EndedState { get; private set; }

    private void Awake()
    {
        GameController controller =
            GetComponent<GameController>();

        SetupState = new GameSetupState(this, controller);
        PlayState = new GamePlayState(this, controller);
        EndedState = new GameEndedState(controller);
    }

    private void Start()
    {
        ChangeState(SetupState);
    }
}