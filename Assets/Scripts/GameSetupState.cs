using UnityEngine;

public class GameSetupState : State
{
    private readonly GameFSM fsm;
    private readonly GameController controller;

    public GameSetupState(
        GameFSM fsm,
        GameController controller)
    {
        this.fsm = fsm;
        this.controller = controller;
    }

    public override void Enter()
    {
        base.Enter();

        controller.ResetGameData();
        Debug.Log("게임 준비 완료");
    }

    public override void Tick()
    {
        base.Tick();

        fsm.ChangeState(fsm.PlayState);
    }
}