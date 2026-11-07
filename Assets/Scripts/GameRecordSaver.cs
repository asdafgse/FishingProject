using UnityEngine;

[RequireComponent(typeof(GameController))]
[RequireComponent(typeof(GameFSM))]
public class GameRecordSaver : MonoBehaviour
{
    private GameController controller;
    private GameFSM gameFSM;
    private bool saved;

    private void Awake()
    {
        controller = GetComponent<GameController>();
        gameFSM = GetComponent<GameFSM>();
    }

    private void LateUpdate()
    {
        // 플레이 중에는 다음 종료 때 저장할 수 있도록 준비해.
        if (gameFSM.CurrentState == gameFSM.PlayState)
        {
            saved = false;
            return;
        }

        if (saved)
        {
            return;
        }

        // 승리하거나 제한 시간이 끝났을 때 저장해.
        if (!controller.HasWon &&
            controller.RemainingTime > 0f)
        {
            return;
        }

        if (SaveManager.Instance == null)
        {
            return;
        }

        SaveManager.Instance.RecordCatch(
            controller.CaughtWeight
        );

        saved = true;
    }
}