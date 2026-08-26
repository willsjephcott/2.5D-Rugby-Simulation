using System;
using UnityEngine;

public class ConversionManager : IContest
{
    public event Action<bool> OnConversionComplete; // true = scored

    Team scoringTeam;
    Ball ball;
    ConversionConfig config;
    NeedleController needle;
    Transform postsCentreTarget;
    Vector3 tryPosition;
    Player kicker;

    public ConversionManager(Team scoringTeam, Ball ball, ConversionConfig config, NeedleController needle, Transform postsCentreTarget, Vector3 tryPosition)
    {
        this.scoringTeam = scoringTeam;
        this.ball = ball;
        this.config = config;
        this.needle = needle;
        this.postsCentreTarget = postsCentreTarget;
        this.tryPosition = tryPosition;
    }

    public void StartConversion()
    {
        PositionKicker();
        AttachBallToKicker();
        ActivateNeedle();
    }

    // IKickContest
    public void NotifyContestResult(bool attackingTeamWon)
    {
        KickBall(attackingTeamWon);
        OnConversionComplete?.Invoke(attackingTeamWon);
    }

    private void PositionKicker()
    {
        kicker = FindKicker();
        if (!ValidateKicker()) return;

        Vector3 kickPos = CalculateKickerPosition();
        kicker.rb.MovePosition(kickPos);
        kicker.SetControlled(false);
        kicker.stateMachine.SM.SetState(new AIIdleState(kicker.stateMachine));

        DebugLogKickerPositioned(kickPos);
    }

    private void AttachBallToKicker()
    {
        if (!ValidateKicker()) return;
        ball.AttachTo(kicker.transform);
    }

    private void ActivateNeedle()
    {
        needle.Activate(this);
    }

    private void KickBall(bool success)
    {
        Vector3 target;

        if (success)
        {
            target = CalculateSuccessTarget();
        }
        else
        {
            target = CalculateFailTarget();
        }
        Transform targetTransform = BuildTargetTransform(target);
        ball.passHandler.ForcePassToTarget(targetTransform);

        DebugLogKick(success, target);
    }

    private Vector3 CalculateKickerPosition()
    {
        Vector3 attackDir = GetAttackDirection();
        return tryPosition - (attackDir * config.kickerDepth);
    }

    private Vector3 CalculateSuccessTarget()
    {
        return postsCentreTarget.position;
    }

    private Vector3 CalculateFailTarget()
    {
        float side = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        return postsCentreTarget.position + GetLateralDirection() * (config.missLateralOffset * side);
    }

    // Creates a temporary scene Transform for RequestPassToPlayer to target.
    // Destroyed after 3 seconds — well past the arc animation duration.  
    private Transform BuildTargetTransform(Vector3 worldPos)
    {
        GameObject target = new GameObject("_ConversionKickTarget");
        target.transform.position = worldPos;
        UnityEngine.Object.Destroy(target, 3f);
        return target.transform;
    }

    private Player FindKicker()
    {
        if (scoringTeam?.players == null || scoringTeam.players.Count == 0) return null;
        return scoringTeam.players[0];
    }

    private Vector3 GetAttackDirection()
    {
        if (scoringTeam == null) return Vector3.forward;
        Vector3 dir = scoringTeam.attackDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        return dir.normalized;
    }

    private Vector3 GetLateralDirection()
    {
        return Vector3.Cross(Vector3.up, GetAttackDirection()).normalized;
    }

    private bool ValidateKicker()
    {
        if (kicker != null) return true;
        DebugLogMissingKicker();
        return false;
    }

    private void DebugLogKickerPositioned(Vector3 pos)
    {
        Debug.Log($"ConversionManager: Kicker {kicker.name} positioned at {pos}");
    }

    private void DebugLogKick(bool success, Vector3 target)
    {
        Debug.Log($"ConversionManager: Kick {(success ? "successful" : "failed")}, target={target}");
    }

    private void DebugLogMissingKicker()
    {
        Debug.LogWarning("ConversionManager: No kicker found on scoring team.");
    }
}