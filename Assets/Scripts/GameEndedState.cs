using UnityEngine;

public class GameEndedState : State
{
    private readonly GameController controller;

    public GameEndedState(GameController controller)
    {
        this.controller = controller;
    }

    public override void Enter()
    {
        base.Enter();

        controller.HUD.DisplayGameEndedMenu();

        Debug.Log(controller.HasWon ? "승리!" : "패배!");
    }

    public override void Exit()
    {
        controller.HUD.HideGameEndedMenu();

        base.Exit();
    }
}