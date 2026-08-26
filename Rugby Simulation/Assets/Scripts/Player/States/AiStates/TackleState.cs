using UnityEngine;

public class TackleState : IPlayerState
{

    readonly PlayerStateMachine fsm;
    readonly Player targetOpponent;

    RuckManager ruckManager;

    public TackleState(PlayerStateMachine fsm, Player targetOpponent)
    {
        this.fsm = fsm;
        this.targetOpponent = targetOpponent;
    }
    public void Enter()
    {
        StartRuck();
    }

    public void Tick()
    {
    }

    public void FixedTick() { }

    public void Exit()
    {
        StopRigidbody();
    }
    private void StartRuck()
    {
        if (!ValidateRuckCanStart())
        {
            return;
        }

        ruckManager = BuildRuckManager();
        SubscribeToRuckComplete();
        ruckManager.StartRuck();
        MatchManager.Instance.RegisterRuck(ruckManager);
    }
    private RuckManager BuildRuckManager()
    {
        Ball ball = GetBall();
        GameConfig config = GetConfig();
        Team attackingTeam = FindAttackingTeam();
        Team defendingTeam = fsm.GetTeam();

        return new RuckManager(targetOpponent, fsm.GetPlayer(), attackingTeam, defendingTeam, ball, config.ruckConfig);
    }
    private void SubscribeToRuckComplete()
    {
        ruckManager.OnRuckComplete += OnRuckComplete;
    }

    private void OnRuckComplete()
    {
        ruckManager.OnRuckComplete -= OnRuckComplete;
        fsm.SM.SetState(new AIIdleState(fsm));
    }

    private Team FindAttackingTeam()
    {
        return targetOpponent?.team;
    }
    private void StopRigidbody()
    {
        Player player = fsm.GetPlayer();
        if (player.rb != null) player.rb.linearVelocity = Vector3.zero;
    }
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }

    private GameConfig GetConfig()
    {
        return MatchManager.Instance?.config;
    }

    private bool ValidateRuckCanStart()
    {
        if (MatchManager.Instance.IsRuckActive()) 
        {
            DebugLogRuckActive();
            return false;
        }
        if (GetBall() == null)
        {
            DebugLogMissingBall();
            return false;
        }
        if (targetOpponent == null)
        {
            DebugLogMissingOpponent();
            return false;
        }
        if (GetConfig()?.ruckConfig == null)
        {
            DebugLogMissingConfig();
            return false;
        }
        return true;
    }

    private void DebugLogRuckActive()
    {
        Debug.LogWarning("TackleState: Ruck already active, ignoring tackle.");
    }
    private void DebugLogMissingBall()
    {
        Debug.LogWarning("TackleState: Ball is null, cannot start ruck.");
    }

    private void DebugLogMissingOpponent()
    {
        Debug.LogWarning("TackleState: Target opponent is null, cannot start ruck.");
    }

    private void DebugLogMissingConfig()
    {
        Debug.LogWarning("TackleState: RuckConfig missing from GameConfig.");
    }
}
