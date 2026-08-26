using UnityEngine;

public class LineoutBoundState: IPlayerState
{
    PlayerStateMachine fsm;

    public LineoutBoundState(PlayerStateMachine fsm)
    {
        this.fsm = fsm;
    }

    public void Enter()
    {
        StopRigidbody();
        SetMovingAnimation(false);
    }

    public void Tick() { }

    public void FixedTick()
    {
        StopRigidbody();
    }

    public void Exit()
    {
        SetMovingAnimation(false);
    }
    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb == null) return;
        player.rb.linearVelocity = Vector3.zero;
        player.rb.angularVelocity = Vector3.zero;
    }

    private void SetMovingAnimation(bool isMoving)
    {
        fsm.GetPlayer().animController?.UpdateMovement(isMoving);
    }
}
