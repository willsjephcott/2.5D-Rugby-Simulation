using UnityEngine;

public class ChaseState: IPlayerState
{
    readonly PlayerStateMachine fsm;
    readonly Player targetOpponent;

    const float chaseSpeedMin = 5f;
    const float chaseSpeedMax = 8f;
    const float tackleRadiusMin = 1.2f;
    const float tackleRadiusMax = 2.8f;

    public ChaseState(PlayerStateMachine fsm, Player targetOpponent)
    {
        this.fsm = fsm;
        this.targetOpponent = targetOpponent;
    }
    public void Enter()
    {
        SetMovingAnimation(true);
        DebugLogEnter();
    }

    public void Tick()
    {
        if (ShouldExitToHumanControl())
        {
            TransitionToIdle();
            return;
        }

        if (ShouldExitBecauseTargetLostBall())
        {
            TransitionToDefensiveLine();
            return;
        }

        if (ShouldEnterTackle())
        {
            TransitionToTackle();
        }
    }

    public void FixedTick()
    {
        if (targetOpponent == null) return;
        MoveTowardsOpponent();
    }

    public void Exit()
    {
        StopRigidbody();
        SetMovingAnimation(false);
    }
    public bool IsTargeting(Player opponent)
    {
        return targetOpponent == opponent;
    }
    private void MoveTowardsOpponent()
    {
        Player player = fsm.GetPlayer();
        Vector3 direction = CalculateDirection();
        Vector3 newPosition = player.rb.position + direction * GetChaseSpeed(player) * Time.fixedDeltaTime;
        player.rb.MovePosition(newPosition);
        UpdateSpriteDirection(direction);
    }

    private Vector3 CalculateDirection()
    {
        Vector3 direction = targetOpponent.transform.position - fsm.GetPlayer().rb.position;
        direction.y = 0f;
        return direction.normalized;
    }
    private bool ShouldEnterTackle()
    {
        if (targetOpponent == null) return false;
        return DistanceToOpponent() <= GetTackleRadius(fsm.GetPlayer());
    }
    private float GetChaseSpeed(Player player)
    {
        float t = player.stats.speed / 100f;
        return Mathf.Lerp(chaseSpeedMin, chaseSpeedMax, t);
    }


    private float GetTackleRadius(Player player)
    {
        float t = player.stats.aggression / 100f;
        return Mathf.Lerp(tackleRadiusMin, tackleRadiusMax, t);
    }
    private bool ShouldExitToHumanControl()
    {
        return fsm.GetPlayer().isControlled;
    }
    private bool ShouldExitBecauseTargetLostBall()
    {
        if (targetOpponent == null) return true;
        return BallPickedUpByOtherPlayer();
    }

    private bool BallPickedUpByOtherPlayer()
    {
        Ball ball = GetBall();
        if (ball == null) return false;
        if (ball.currentHolder == null) return false; // loose ball - stay in chase
        return ball.currentHolder != targetOpponent.transform; // someone else has it
    }

    private float DistanceToOpponent()
    {
        Vector3 diff = targetOpponent.transform.position - fsm.GetPlayer().rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    private void TransitionToTackle()
    {
        DebugLogTackleEntry();
        fsm.SM.SetState(new TackleState(fsm, targetOpponent));
    }

    private void TransitionToIdle()
    {
        fsm.SM.SetState(new IdleState(fsm));
    }
    private void TransitionToDefensiveLine()
    {
        fsm.SM.SetState(new AIIdleState(fsm));
    }
    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb != null)
            player.rb.linearVelocity = Vector3.zero;
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
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }
    private void DebugLogEnter()
    {
        Debug.Log($"{fsm.GetPlayer().name} entering ChaseState, chasing {targetOpponent?.name}. Distance: {DistanceToOpponent()}");
    }
    private void DebugLogTackleEntry()
    {
        Debug.Log($"{fsm.GetPlayer().name} entering TackleState, tackling {targetOpponent?.name}. Distance: {DistanceToOpponent()}");
    }
}
