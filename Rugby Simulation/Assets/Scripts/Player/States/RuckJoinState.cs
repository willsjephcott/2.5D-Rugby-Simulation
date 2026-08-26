using UnityEngine;

public class RuckJoinState: IPlayerState
{
    readonly PlayerStateMachine fsm;

    RuckManager ruckManager;
    Vector3 targetPosition;
    bool isScrumhalf;

    const float MoveSpeed = 5f;
    const float ArrivalThreshold = 0.3f;

    public RuckJoinState(PlayerStateMachine fsm, RuckManager ruckManager, Vector3 targetPosition, bool isScrumhalf)
    {
        this.fsm = fsm;
        this.ruckManager = ruckManager;
        this.targetPosition = targetPosition;
        this.isScrumhalf = isScrumhalf;
    }
    public void Enter()
    {
        SetMovingAnimation(true);
    }

    public void Tick()
    {
        if (HasArrived())
        {
            NotifyArrival();
        }
    }

    public void FixedTick()
    {
        if (!HasArrived())
        {
            MoveTowardsTarget();
        }
    }

    public void Exit()
    {
        SetMovingAnimation(false);
    }
    private void MoveTowardsTarget()
    {
        Player player = fsm.GetPlayer();
        Vector3 direction = CalculateDirection();
        Vector3 newPosition = player.rb.position + direction * MoveSpeed * Time.fixedDeltaTime;
        player.rb.MovePosition(newPosition);
        UpdateSpriteDirection(direction);
    }

    private Vector3 CalculateDirection()
    {
        Vector3 direction = targetPosition - fsm.GetPlayer().rb.position;
        direction.y = 0f;
        return direction.normalized;
    }

    private float CalculateDistance()
    {
        Vector3 diff = targetPosition - fsm.GetPlayer().rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    private bool HasArrived()
    {
        return CalculateDistance() <= ArrivalThreshold;
    }

    private void NotifyArrival()
    {
        if (isScrumhalf)
        {
            ruckManager.NotifyScrumhalfArrived();
        }
        else
        {
            NotifySupportArrived();
            EnterRuckBoundState();
        }
    }
    private void NotifySupportArrived()
    {
        Team team = fsm.GetTeam();
        ruckManager.NotifySupportArrived(fsm.GetPlayer(), team);
    }

    private void EnterRuckBoundState()
    {
        fsm.SM.SetState(new RuckBoundState(fsm));
    }

    private void SetMovingAnimation(bool isMoving)
    {
        fsm.GetPlayer().animController?.UpdateMovement(isMoving);
    }

    private void UpdateSpriteDirection(Vector3 direction)
    {
        Player player = fsm.GetPlayer();
        if (player.sr == null) return;
        if (direction.z < -0.01f) player.sr.flipX = false;
        if (direction.z > 0.01f) player.sr.flipX = true;
    }
}
