using UnityEngine;

public class AIIdleState : IPlayerState
{
    readonly PlayerStateMachine fsm;

    public AIIdleState(PlayerStateMachine fsm)
    {
        this.fsm = fsm;
    }

    public void Enter()
    {
        UpdateAnimatorToIdle();
    }

    public void Tick()
    {
        // Future: check if possession changed, assign defend state etc.
    }

    public void FixedTick() { }

    public void Exit() { }

    private void UpdateAnimatorToIdle()
    {
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }
}