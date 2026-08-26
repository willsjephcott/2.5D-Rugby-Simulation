using UnityEngine;

public class HumanDefendState : IPlayerState
{
    readonly PlayerStateMachine fsm;
    readonly Team opposingTeam;

    const float TackleRange = 1.2f;

    public HumanDefendState(PlayerStateMachine fsm, Team opposingTeam)
    {
        this.fsm = fsm;
        this.opposingTeam = opposingTeam;
    }
    public void Enter()
    {
        SetMovingAnimation(false);
    }

    public void Tick()
    {
        if (ShouldExitToAI())
        {
            TransitionToAIIdle();
            return;
        }

        if (HasTackleBeenRequested())
        {
            HandleTackleRequest();
        }
    }

    public void FixedTick()
    {
        ProcessMovement();
    }

    public void Exit()
    {
        StopRigidbody();
        SetMovingAnimation(false);
        fsm.ClearTackleRequest();
    }
    private void ProcessMovement()
    {
        Vector2 input = fsm.moveInput;
        bool sprint = fsm.sprintInput;
        fsm.GetPlayer().movement.Move(input, sprint);
        UpdateMovementAnimation(input);
    }

    private void UpdateMovementAnimation(Vector2 input)
    {
        bool isMoving = input.sqrMagnitude > 0.01f;
        fsm.GetPlayer().animController?.UpdateMovement(isMoving);
    }
    private void HandleTackleRequest()
    {
        fsm.ClearTackleRequest();

        Player nearestOpponent = FindNearestOpponentInRange();

        if (nearestOpponent == null)
        {
            DebugLogTackleMissed();
            return;
        }

        TransitionToTackle(nearestOpponent);
    }

    private Player FindNearestOpponentInRange()
    {
        if (opposingTeam?.players == null) return null;

        Player nearest = null;
        float nearestDistance = TackleRange;

        foreach (Player opponent in opposingTeam.players)
        {
            if (opponent == null) continue;

            float distance = CalculateDistanceTo(opponent);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = opponent;
            }
        }

        return nearest;
    }

    private float CalculateDistanceTo(Player target)
    {
        Vector3 diff = target.transform.position - fsm.GetPlayer().rb.position;
        diff.y = 0f;
        return diff.magnitude;
    }
    private bool HasTackleBeenRequested()
    {
        return fsm.tackleRequested;
    }

    private bool ShouldExitToAI()
    {
        return !fsm.GetPlayer().isControlled;
    }
    private void TransitionToTackle(Player opponent)
    {
        fsm.SM.SetState(new TackleState(fsm, opponent));
    }

    private void TransitionToAIIdle()
    {
        fsm.SM.SetState(new AIIdleState(fsm));
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
    private void DebugLogTackleMissed()
    {
        Debug.Log($"{fsm.GetPlayer().name} attempted tackle but no opponent in range.");
    }
}
