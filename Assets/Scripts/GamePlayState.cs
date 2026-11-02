using UnityEngine;

public class GamePlayState : State
{
    private readonly GameFSM fsm;
    private readonly GameController controller;

    public GamePlayState(
        GameFSM fsm,
        GameController controller)
    {
        this.fsm = fsm;
        this.controller = controller;
    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("낚시 시작!");
    }

    public override void Tick()
    {
        base.Tick();

        controller.RemainingTime = Mathf.Max(
            0f,
            controller.RemainingTime - Time.deltaTime
        );

        if (controller.CaughtWeight >= controller.TargetWeight)
        {
            controller.HasWon = true;
            fsm.ChangeState(fsm.EndedState);
            return;
        }

        if (controller.RemainingTime <= 0f)
        {
            controller.HasWon = false;
            fsm.ChangeState(fsm.EndedState);
        }
    }
}