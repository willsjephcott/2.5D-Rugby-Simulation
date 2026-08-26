using UnityEngine;
using System.Collections.Generic;

public class DefensiveLineState : IPlayerState
{
    readonly PlayerStateMachine fsm;

    Player assignedOpponent;
    List<Player> defensiveLine;

    const float moveSpeed = 4f;
    const float chaseEntryDistanceMin = 1.5f;
    const float chaseEntryDistanceMax = 4f;
    const float arrivalThreshold = 0.1f;

    public DefensiveLineState(PlayerStateMachine fsm, Player assignedOpponent, List<Player> defensiveLine)
    {
        this.fsm = fsm;
        this.assignedOpponent = assignedOpponent;
        this.defensiveLine = defensiveLine;
    }
    public void Enter()
    {
        SetMovingAnimation(true);
    }
    public void Tick()
    {
        if (ShouldExitToHumanControl())
        {
            TransitionToIdle();
            return;
        }

        if (ShouldEnterChase())
        {
            TransitionToChase();
            return;
        }

        if (HasOpponentBrokenThrough())
        {
            TransitionToReform();
        }
    }
    public void FixedTick()
    {
        if (!ValidateOpponent()) return;

        Vector3 target = CalculateLineTarget();
        MoveTowardsTarget(target);
    }
    public void Exit()
    {
        StopRigidbody();
        SetMovingAnimation(false);
    }
    public void UpdateAssignment(Player newOpponent, List<Player> newLine)
    {
        assignedOpponent = newOpponent;
        defensiveLine = newLine;
    }
    private bool HasOpponentBrokenThrough()
    {
        if (!ValidateOpponent()) return false;

        Player player = fsm.GetPlayer();
        Vector3 attackDir = GetAttackDirection();
        Vector3 toOpponent = assignedOpponent.transform.position - player.rb.position;
        toOpponent.y = 0f;

        // Opponent is behind = they've beaten the line
        float dot = Vector3.Dot(toOpponent.normalized, attackDir);
        return dot > 0.5f && toOpponent.magnitude > 2f;
    }
    private Vector3 GetAttackDirection()
    {
        if (assignedOpponent?.team == null) return Vector3.forward;
        Vector3 dir = assignedOpponent.team.attackDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            return Vector3.forward;
        }

        return dir.normalized;
    }

    private void TransitionToReform()
    {
        fsm.SM.SetState(new DefensiveReformState(fsm, assignedOpponent));
    }
    private Vector3 CalculateLineTarget()
    {
        float targetX = GetOpponentX();
        float targetY = GetPlayerY();
        float targetZ = CalculateAverageLineZ();

        return new Vector3(targetX, targetY, targetZ);
    }
    private float GetOpponentX()
    {
        return assignedOpponent.transform.position.x;
    }
    private float GetPlayerY()
    {
        return fsm.GetPlayer().rb.position.y;
    }
    private float CalculateAverageLineZ()
    {
        if (!HasValidDefensiveLine()) return GetPlayerZ();

        return SumLineZ() / CountValidLineMembers();
    }
    private bool HasValidDefensiveLine()
    {
        return defensiveLine != null && defensiveLine.Count > 0 && CountValidLineMembers() > 0;
    }
    private float SumLineZ()
    {
        float total = 0f;
        foreach (Player p in defensiveLine)
        {
            if (p != null) total += p.transform.position.z;
        }
        return total;
    }
    private int CountValidLineMembers()
    {
        int count = 0;
        foreach (Player p in defensiveLine)
        {
            if (p != null) count++;
        }
        return count;
    }
    private float GetPlayerZ()
    {
        return fsm.GetPlayer().rb.position.z;
    }
    private void MoveTowardsTarget(Vector3 target)
    {
        float distance = CalculateDistance(target);

        if (HasArrivedAtTarget(distance))
        {
            StopRigidbody();
            return;
        }

        Vector3 direction = CalculateDirection(target);
        ApplyMovement(direction);
        UpdateSpriteDirection(direction);
    }
    private float CalculateDistance(Vector3 target)
    {
        Vector3 diff = target - fsm.GetPlayer().rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }
    private bool HasArrivedAtTarget(float distance)
    {
        return distance < arrivalThreshold;
    }
    private Vector3 CalculateDirection(Vector3 target)
    {
        Vector3 direction = target - fsm.GetPlayer().rb.position;
        direction.y = 0f;
        return direction.normalized;
    }
    private void ApplyMovement(Vector3 direction)
    {
        Player player = fsm.GetPlayer();
        Vector3 newPosition = player.rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        player.rb.MovePosition(newPosition);
    }
    private bool ShouldEnterChase()
    {
        if (!ValidateOpponent()) return false;
        if (MatchManager.Instance != null && MatchManager.Instance.IsRuckActive()) return false;

        //if (IsOpponentAlreadyBeingChased()) return false;
        return DistanceToOpponent() <= GetChaseEntryDistance();
    }
    private bool IsOpponentAlreadyBeingChased()
    {
        // Check if any other player is already chasing our assigned opponent
        Player[] allPlayers = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player p in allPlayers)
        {
            if (p == fsm.GetPlayer()) continue;
            if (p.stateMachine?.SM?.CurrentState is ChaseState chase && chase.IsTargeting(assignedOpponent)) return true;
        }
        return false;
    }
    private float GetChaseEntryDistance()
    {
        float t = fsm.GetPlayer().stats.aggression / 100f;
        return Mathf.Lerp(chaseEntryDistanceMin, chaseEntryDistanceMax, t);
    }
    private bool ShouldExitToHumanControl()
    {
        return fsm.GetPlayer().isControlled;
    }
    private float DistanceToOpponent()
    {
        Vector3 diff = assignedOpponent.transform.position - fsm.GetPlayer().rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }
    private void TransitionToChase()
    {
        fsm.SM.SetState(new ChaseState(fsm, assignedOpponent));
    }
    private void TransitionToIdle()
    {
        fsm.SM.SetState(new IdleState(fsm));
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
    private bool ValidateOpponent()
    {
        return assignedOpponent != null;
    }
    // Gizmo helpers — called by DefensiveLineController.OnDrawGizmos
    public Vector3 GetDebugTarget()
    {
        if (!ValidateOpponent()) return Vector3.zero;
        return CalculateLineTarget();
    }

    public Player GetDebugOpponent()
    {
        return assignedOpponent;
    }
}