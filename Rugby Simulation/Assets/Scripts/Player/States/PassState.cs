using UnityEngine;

public class PassState : IPlayerState
{
    readonly PlayerStateMachine fsm;
    GameConfig config;

    Player targetPlayer;
    Vector3 groundTargetPos;

    bool hasPassed;
    float timer;

    public PassState(PlayerStateMachine fsm) 
    {
        this.fsm = fsm; 
    }

    public void Enter()
    {
        if (!ValidateCanPass())
        {
            ExitToIdleState();
            return;
        }

        InitialisePassState();
        DeterminePassTarget();
        StartPassAnimation();
    }
    public void Tick()
    {
        AdvanceTimer();

        if (ShouldExecutePass())
        {
            ExecutePass();
        }

        if (ShouldExitState())
        {
            TransitionToNextState();
        }
    }
    public void FixedTick() { }
    public void Exit() { }

    private void InitialisePassState()
    {
        config = FindConfig();
        timer = 0;
        hasPassed = false;
    }
    private void DeterminePassTarget()
    {
        Player passer = fsm.GetPlayer();
        Team team = fsm.GetTeam();

        targetPlayer = FindPlayerTarget(passer, team);

        if (HasPlayerTarget())
        {
            groundTargetPos = targetPlayer.transform.position;
        }
        else
        {
            groundTargetPos = CalculateGroundTarget(passer, team);
        }
    }
    private Player FindPlayerTarget(Player passer, Team team)
    {
        if (team == null) return null;
        return team.FindBestPassTarget(passer, fsm.passLeftSide);
    }

    private Vector3 CalculateGroundTarget(Player passer, Team team)
    {
        if (team == null) return passer.transform.position;
        return team.CalculateGroundPassTarget(passer, fsm.passLeftSide);
    }
    private void StartPassAnimation()
    {
        fsm.ClearPassRequest();
        TriggerPlayerPassAnimation();
    }
    private void TriggerPlayerPassAnimation()
    {
        Player player = fsm.GetPlayer();
        player.animController?.TriggerPass();
    }
    private void AdvanceTimer()
    {
        timer += Time.deltaTime;
    }
    private bool ShouldExecutePass()
    {
        return !hasPassed && timer >= config.passAnimationDelay;
    }
    private void ExecutePass()
    {
        Ball ball = GetBall();
        if (!ValidateBallExists(ball)) return;

        Player passer = fsm.GetPlayer();
        float handlingStat = GetPasserHandlingStat(passer);
        float difficultyModifier = 1f;

        if (HasPlayerTarget())
        {
            RequestPlayerPass(ball, handlingStat, difficultyModifier);
        }
        else
        {
            RequestGroundPass(ball, handlingStat, difficultyModifier);
        }

        hasPassed = true;
    }
    private void RequestPlayerPass(Ball ball, float handlingStat, float difficultyModifier)
    {
        ball.passHandler.RequestPassToPlayer(targetPlayer.transform, handlingStat, difficultyModifier);
    }
    private void RequestGroundPass(Ball ball, float handlingStat, float difficultyModifier)
    {
        ball.passHandler.RequestPassToGround(groundTargetPos, handlingStat, difficultyModifier);
    }
    private float GetPasserHandlingStat(Player passer)
    {
        if (passer != null) return passer.handlingStat;
        if (config != null) return config.defaultHandlingStat;
        return 70f;
    }
    private bool ShouldExitState()
    {
        return timer >= config.passStateExitTime;
    }
    private void TransitionToNextState()
    {
        IPlayerState nextState = DetermineNextState();
        fsm.GetPlayer().stateMachine.SM.SetState(nextState);
    }
    private IPlayerState DetermineNextState()
    {
        if (IsPlayerMoving())
        {
            return new MoveState(fsm);
        }
        else
        {
            return new IdleState(fsm);
        }
    }
    private bool IsPlayerMoving()
    {
        return fsm.moveInput.sqrMagnitude > 0.01f;
    }
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }
    private GameConfig FindConfig()
    {
        if (MatchManager.Instance != null)
        {
            return MatchManager.Instance.config;
        }
        return Object.FindAnyObjectByType<GameConfig>();
    }
    private bool HasPlayerTarget()
    {
        return targetPlayer != null;
    }
    private void ExitToIdleState()
    {
        fsm.ClearPassRequest();
        fsm.GetPlayer().stateMachine.SM.SetState(new IdleState(fsm));
    }
    private bool ValidateCanPass()
    {
        Player player = fsm.GetPlayer();
        Ball ball = GetBall();

        if (!ValidateBallExists(ball))
        {
            return false;
        }

        if (!ValidatePlayerHasBall(player, ball))
        {
            DebugPlayerDoesNotHaveBall(player);
            return false;
        }

        return true;
    }
    private bool ValidateBallExists(Ball ball)
    {
        if (ball == null)
        {
            DebugBallBecameNull();
            ExitToIdleState();
            return false;
        }
        return true;
    }
    private bool ValidatePlayerHasBall(Player player, Ball ball)
    {
        return ball.currentHolder == player.transform;
    }
    private void DebugPlayerDoesNotHaveBall(Player player)
    {
        Debug.LogWarning($"{player.name} tried to pass but doesn't have the ball");
    }
    private void DebugBallBecameNull()
    {
        Debug.LogWarning("Ball became null during pass state");
    }


}

    /*public void Enter()
    {
        Debug.Log("Entered Pass State");
        config = MatchManager.Instance != null ? MatchManager.Instance.config : Object.FindAnyObjectByType<GameConfig>();

        var player = fsm.GetPlayer();
        var team = fsm.GetTeam();
        var ball = MatchManager.Instance?.ball;

        // Safety: only holder can pass  TODO turn this into method (debug method)
        if (ball == null || ball.currentHolder != player.transform)
        {
            Debug.LogWarning($"{player.name} tried to pass but doesn't have the ball");
            fsm.ClearPassRequest();
            player.stateMachine.SM.SetState(new IdleState(fsm));
            return;
        }

        // Find target or calculate ground position
        targetPlayer = team?.FindBestPassTarget(player, fsm.passLeftSide);

        if (targetPlayer != null)
        {
            groundTargetPos = targetPlayer.transform.position;
        }
        else if (team != null)
        {
            groundTargetPos = team.CalculateGroundPassTarget(player, fsm.passLeftSide);
        }
        else
        {
            groundTargetPos = player.transform.position;

        }

        fsm.ClearPassRequest();
        player.animController?.TriggerPass();

        hasPassed = false;
        timer = 0f;
    }

    public void Tick()
    {
        timer += Time.deltaTime;

        // Execute pass after animation delay
        if (!hasPassed && timer >= config.passAnimationDelay)
        {
            var player = fsm.GetPlayer();
            var ball = MatchManager.Instance?.ball;
            if (ball == null)
            {
                Debug.LogWarning("Ball became null during pass");
                fsm.GetPlayer().stateMachine.SM.SetState(new IdleState(fsm));
                return;
            }

            //If player has a handling stat, if not use config or use 70 if that doesn't work for some reason (haven't tested yet so just incase)
            float handlingStat = player != null ? player.handlingStat :(config != null ? config.defaultHandlingStat : 70f);

            float difficultyModifier = 1f;

            if (targetPlayer != null)
            {
                ball.passHandler.PassToPlayer(targetPlayer.transform,handlingStat,difficultyModifier);
            }
            else
            {
                ball.passHandler.PassToGround(groundTargetPos,handlingStat,difficultyModifier);
            }

            hasPassed = true;
        }

        // Exit after animation completes
        if (timer >= config.passStateExitTime)
        {
            IPlayerState nextState;
           if (fsm.moveInput.sqrMagnitude > 0.01f)
            {
                nextState = new MoveState(fsm);
            }
            else
            {
                nextState = new IdleState(fsm);
            }

            fsm.GetPlayer().stateMachine.SM.SetState(nextState);
        }
    }

    public void Exit() { }*/

