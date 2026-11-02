using UnityEngine;

public abstract class State
{
    public float StateDuration { get; private set; }

    public virtual void Enter()
    {
        StateDuration = 0f;
    }

    public virtual void Tick()
    {
        StateDuration += Time.deltaTime;
    }

    public virtual void FixedTick()
    {
    }

    public virtual void Exit()
    {
    }
}