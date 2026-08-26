using Unity.VisualScripting;
using UnityEngine;

public class IdleState: IPlayerState
{

    readonly PlayerStateMachine fsm;

    public IdleState(PlayerStateMachine fsm)
    {
        this.fsm = fsm;
    }
    public void Enter()
    {
        UpdateAnimatorToIdle();
    }

    public void Tick()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.IsConversionActive()) return;
        if (ShouldTransitionToMove())
        {
            TransitionToMoveState();
            return;
        }

        if (ShouldTransitionToPass())
        {
            TransitionToPassState();
            return;
        }
    }
    public void FixedTick() { }

    public void Exit() { }

    private void UpdateAnimatorToIdle()
    {
        Player player = fsm.GetPlayer();
        // ?. means if animCOntroller is null it doesn't crash
        player.animController?.UpdateMovement(false);
    }

    private bool ShouldTransitionToMove()
    {
        return fsm.moveInput.sqrMagnitude > 0.01f;
    }

    private bool ShouldTransitionToPass()
    {
        return fsm.passRequested;
    }

    private void TransitionToMoveState()
    {
        fsm.GetPlayer().stateMachine.SM.SetState(new MoveState(fsm));
    }

    // Switches to pass state
    private void TransitionToPassState()
    {
        fsm.GetPlayer().stateMachine.SM.SetState(new PassState(fsm));
    }
    /*public  void Enter()
    {
        // Question mark means it doesn't crash if animController is null
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }

    public  void Tick()
    {
        if(fsm.moveInput.sqrMagnitude > 0.01f)
        {
            fsm.GetPlayer().stateMachine.SM.SetState(new MoveState(fsm));
            return;
        }

        // Check for pass request
        if (fsm.passRequested)
        {
            fsm.GetPlayer().stateMachine.SM.SetState(new PassState(fsm));
            return;
        }
    }

    public void Exit() { }*/
}
