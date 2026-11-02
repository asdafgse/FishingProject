using UnityEngine;

public abstract class StateMachineMB : MonoBehaviour
{
    public State CurrentState { get; private set; }
    public State PreviousState { get; private set; }

    private bool isTransitioning;

    public void ChangeState(State newState)
    {
        if (newState == null ||
            newState == CurrentState ||
            isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        CurrentState?.Exit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.Enter();

        isTransitioning = false;
    }

    public void ChangeStateToPrevious()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }

    public void ClearPreviousState()
    {
        PreviousState = null;
    }

    protected virtual void Update()
    {
        CurrentState?.Tick();
    }

    protected virtual void FixedUpdate()
    {
        CurrentState?.FixedTick();
    }
}