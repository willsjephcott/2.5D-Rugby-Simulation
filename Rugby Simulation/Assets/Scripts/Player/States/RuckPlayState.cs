using UnityEngine;

public class RuckPlayState: IPlayerState
{
    readonly PlayerStateMachine fsm;
    RuckManager ruckManager;

    public RuckPlayState(PlayerStateMachine fsm, RuckManager ruckManager)
    {
        this.fsm = fsm;
        this.ruckManager = ruckManager;
    }

    public void Enter()
    {
        StopRigidbody();
        SetMovingAnimation(false);
        ZeroMoveInput();
    }
    public void Tick()
    {
        if (HasPassBeenRequested())
        {
            ExecutePass();
        }
    }

    public void FixedTick()
    {
        StopRigidbody();
    }

    public void Exit()
    {
        fsm.ClearPassRequest();
    }

    private bool HasPassBeenRequested()
    {
        return fsm.passRequested;
    }

    private void ExecutePass()
    {
        fsm.ClearPassRequest();
        TransitionToPassState();
        ruckManager.NotifyPassMade();
    }

    private void TransitionToPassState()
    {
        fsm.SM.SetState(new PassState(fsm));
    }

    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb == null) return;
        player.rb.linearVelocity = Vector3.zero;
    }

    private void SetMovingAnimation(bool isMoving)
    {
        fsm.GetPlayer().animController?.UpdateMovement(isMoving);
    }

    private void ZeroMoveInput()
    {
        fsm.SetInput(Vector2.zero, false);
    }
}
