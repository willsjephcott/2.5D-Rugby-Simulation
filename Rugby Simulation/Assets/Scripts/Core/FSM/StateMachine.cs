using UnityEngine;

public class StateMachine
{
    public IPlayerState CurrentState { get; private set; }
    public void SetState(IPlayerState newState)
    {
        if (newState == CurrentState) return;

        // ?. (null conditional operator) only do this if thing on left is NOT null
        CurrentState?.Exit();
        CurrentState = newState;
        // Enter new state
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
    public void FixedTick()
    {
        CurrentState?.FixedTick();
    }
}
