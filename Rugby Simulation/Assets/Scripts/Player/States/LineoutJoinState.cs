using UnityEngine;

public class LineoutJoinState: IPlayerState
{
    PlayerStateMachine fsm;
    LineoutManager lineoutManager;
    Vector3 targetPosition;
    bool isAttacking;

    float moveSpeed;
    float arrivalThreshold;
    public LineoutJoinState(PlayerStateMachine fsm, LineoutManager lineoutManager, Vector3 targetPosition, bool isAttacking, float moveSpeed, float arrivalThreshold)
    {
        this.fsm = fsm;
        this.lineoutManager = lineoutManager;
        this.targetPosition = targetPosition;
        this.isAttacking = isAttacking;
        this.moveSpeed = moveSpeed;
        this.arrivalThreshold = arrivalThreshold;
    }
    public void Enter()
    {
        Debug.Log($"{fsm.GetPlayer().name} entered LineoutJoinState, target={targetPosition}");
        SetMovingAnimation(true);
    }

    public void Tick()
    {
        if (HasArrived())
        {
            Debug.Log($"{fsm.GetPlayer().name} HasArrived triggered");
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
        StopRigidbody();
    }
    private void MoveTowardsTarget()
    {
        Player player = fsm.GetPlayer();
        Vector3 direction = CalculateDirection();
        Vector3 newPosition = player.rb.position + direction * moveSpeed * Time.fixedDeltaTime;
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
        return CalculateDistance() <= arrivalThreshold;
    }

    private void NotifyArrival()
    {
        Player player = fsm.GetPlayer();
        Debug.Log($"{fsm.GetPlayer().name} arrived at lineout slot");
        fsm.SM.SetState(new LineoutBoundState(fsm));
        lineoutManager.NotifyPlayerArrived(fsm.GetPlayer(), isAttacking);
    }

    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb != null) player.rb.linearVelocity = Vector3.zero;
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
