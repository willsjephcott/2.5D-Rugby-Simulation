using UnityEngine;

public class MoveState : IPlayerState
{

    readonly PlayerStateMachine fsm;

    public MoveState(PlayerStateMachine fsm) 
    {
        this.fsm = fsm; 
    }

    public void Enter()
    {
        UpdateAnimatorToMoving();
    }

    public void Tick()
    {
        if (ShouldTransitionToIdle())
        {
            TransitionToIdleState();
            return;
        }
    }
    public void FixedTick()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.IsConversionActive()) return;
        ExecuteMovement();
    }
    public void Exit()
    {
        UpdateAnimatorToIdle();
    }

    private void UpdateAnimatorToMoving()
    {
        Player player = fsm.GetPlayer();
        player.animController?.UpdateMovement(true);
    }

    private void UpdateAnimatorToIdle()
    {
        Player player = fsm.GetPlayer();
        player.animController?.UpdateMovement(false);
    }

    private void ExecuteMovement()
    {
        Player player = fsm.GetPlayer();
        player.movement?.Move(fsm.moveInput, fsm.sprintInput);
    }

    private bool ShouldTransitionToIdle()
    {
        return fsm.moveInput.sqrMagnitude <= 0.01f;
    }

    private void TransitionToIdleState()
    {
        Player player = fsm.GetPlayer();
        player.stateMachine.SM.SetState(new IdleState(fsm));
    }
    /*public void Enter()
    {
        fsm.GetPlayer().animController?.UpdateMovement(true);
    }
    public void Tick()
    {
        Debug.Log("MoveState Tick");
        var player = fsm.GetPlayer();

        // Execute movement
        player.movement?.Move(fsm.moveInput, fsm.sprintInput);

        // Check for stop
        if (fsm.moveInput.sqrMagnitude <= 0.01f)
        {
            player.stateMachine.SM.SetState(new IdleState(fsm));
            return;
        }
    }
    public void Exit()
    {
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }*/
}
