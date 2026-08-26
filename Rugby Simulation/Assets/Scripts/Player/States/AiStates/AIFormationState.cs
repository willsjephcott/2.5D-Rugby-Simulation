using UnityEngine;

public class AIFormationState: IPlayerState
{
    readonly PlayerStateMachine fsm;

    Vector3 targetPosition;
    bool hasTarget;

    const float ArrivalThreshold = 0.3f;
    const float FollowSpeed = 4f;

    public AIFormationState(PlayerStateMachine fsm)
    {
        this.fsm = fsm;
    }
    public void Enter()
    {
        fsm.GetPlayer().animController?.UpdateMovement(true);
    }

    public void Tick()
    {
        if (ShouldExitToHumanControl())
        {
            TransitionToIdleForHuman();
        }
    }

    public void FixedTick()
    {
        if (!hasTarget) return;

        float distance = CalculateDistance();

        if (HasArrived(distance))
        {
            StopAndIdle();
            return;
        }

        MoveTowardsTarget();
    }

    public void Exit()
    {
        StopRigidbody();
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }
    public void UpdateTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
        hasTarget = true;
    }
    public void ClearTarget()
    {
        hasTarget = false;
    }
    private void MoveTowardsTarget()
    {
        Player player = fsm.GetPlayer();

        Vector3 direction = CalculateDirection();
        Vector3 newPosition = player.rb.position + direction * FollowSpeed * Time.fixedDeltaTime;

        player.rb.MovePosition(newPosition);
        UpdateSpriteDirection(direction);
    }
    private Vector3 CalculateDirection()
    {
        Player player = fsm.GetPlayer();
        Vector3 direction = targetPosition - player.rb.position;
        direction.y = 0f;
        return direction.normalized;
    }
    private float CalculateDistance()
    {
        Player player = fsm.GetPlayer();
        Vector3 diff = targetPosition - player.rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    private bool HasArrived(float distance)
    {
        return distance < ArrivalThreshold;
    }

    private void StopAndIdle()
    {
        StopRigidbody();
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }
    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb != null)
        {
            player.rb.linearVelocity = Vector3.zero;
        }
    }
    private void UpdateSpriteDirection(Vector3 direction)
    {
        Player player = fsm.GetPlayer();
        if (player.sr == null) return;

        if (direction.z < -0.01f) player.sr.flipX = false;
        if (direction.z > 0.01f) player.sr.flipX = true;
    }
    private bool ShouldExitToHumanControl()
    {
        return fsm.GetPlayer().isControlled;
    }
    private void TransitionToIdleForHuman()
    {
        fsm.GetPlayer().stateMachine.SM.SetState(new IdleState(fsm));
    }
    public void DebugDrawTarget(Vector3 playerPosition)
    {
        if (!hasTarget) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        Gizmos.DrawLine(playerPosition, targetPosition);
    }

}
