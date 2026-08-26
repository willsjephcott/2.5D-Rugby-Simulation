using UnityEngine;
using System.Collections.Generic;

public class DefensiveReformState: IPlayerState
{
    readonly PlayerStateMachine fsm;
    Player trackedCarrier;

    const float ReformDepth = 1f; // how far behind carrier to reform
    const float ReformSpeed = 8f; // sprint speed to get back
    const float ArrivalThreshold = 0.5f;
    const float ReEngageDistance = 1f; // once reformed and carrier is close, re-enter line

    public DefensiveReformState(PlayerStateMachine fsm, Player carrier)
    {
        this.fsm = fsm;
        this.trackedCarrier = carrier;
    }
    public void Enter()
    {
        fsm.GetPlayer().animController?.UpdateMovement(true);
    }

    public void Tick()
    {
        if (ShouldExitToHumanControl())
        {
            fsm.SM.SetState(new IdleState(fsm));
            return;
        }

        // Once carrier is close enough again, go back to defensive line state
        if (HasReEngaged())
        {
            fsm.SM.SetState(new AIIdleState(fsm));
        }
    }
    public void FixedTick()
    {
        if (trackedCarrier == null) return;
        MoveToReformPosition();
    }

    public void Exit()
    {
        StopRigidbody();
        fsm.GetPlayer().animController?.UpdateMovement(false);
    }

    private void MoveToReformPosition()
    {
        Vector3 target = CalculateReformTarget();
        Player player = fsm.GetPlayer();

        Vector3 direction = target - player.rb.position;
        direction.y = 0f;

        if (direction.magnitude < ArrivalThreshold)
        {
            StopRigidbody();
            return;
        }

        Vector3 newPosition = player.rb.position + direction.normalized * ReformSpeed * Time.fixedDeltaTime;
        player.rb.MovePosition(newPosition);
        UpdateSpriteDirection(direction.normalized);
    }
    private Vector3 CalculateReformTarget()
    {
        Vector3 attackDir = GetAttackDirection();
        // Position behind the carrier (in the direction they came from)
        return trackedCarrier.transform.position + attackDir * ReformDepth;
    }

    private bool HasReEngaged()
    {
        if (trackedCarrier == null) return true;

        Vector3 attackDir = GetAttackDirection();
        Vector3 toDefender = fsm.GetPlayer().rb.position - trackedCarrier.transform.position;
        toDefender.y = 0f;

        // Only re-engage if we're actually ahead of the carrier now
        float dot = Vector3.Dot(toDefender.normalized, attackDir);
        float distance = toDefender.magnitude;

        return dot > 0.3f && distance <= ReEngageDistance;
    }
    private bool ShouldExitToHumanControl()
    {
        return fsm.GetPlayer().isControlled;
    }

    private Vector3 GetAttackDirection()
    {
        if (trackedCarrier?.team == null) return Vector3.forward;
        Vector3 dir = trackedCarrier.team.attackDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return Vector3.forward;

        return dir.normalized;
    }
    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb != null) player.rb.linearVelocity = Vector3.zero;
    }

    private void UpdateSpriteDirection(Vector3 direction)
    {
        Player player = fsm.GetPlayer();
        if (player.sr == null) return;
        if (direction.z < -0.01f) player.sr.flipX = false;
        if (direction.z > 0.01f) player.sr.flipX = true;
    }
}
