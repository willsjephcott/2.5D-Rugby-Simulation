using System;
using System.Collections.Generic;
using UnityEngine;

public class RuckManager
{
    public enum RuckPhase
    {
        Forming, // Players go to ruck
        Won, // Scrumhalf runs to ruck to play ball
        PlayingBall, //Human takes control
        Unloading // Stack releases players
    }

    public RuckPhase Phase {  get; private set; }
    public Vector3 RuckPosition { get; private set; }
    public Player GetScrumhalf()
    {
        return scrumhalf;
    }

    Team attackingTeam;
    Team defendingTeam;
    Ball ball;
    RuckConfig config;

    RuckStack attackingStack;
    RuckStack defendingStack;

    Player carrier;
    Player tackler;
    Player scrumhalf;

    int attackingSupportExpected;
    int defendingSupportExpected;
    int attackingSupportArrived;
    int defendingSupportArrived;

    public event Action OnRuckComplete;

    public RuckManager(Player carrier, Player tackler, Team attackingTeam, Team defendingTeam, Ball ball, RuckConfig config)
    {
        this.carrier = carrier;
        this.tackler = tackler;
        this.attackingTeam = attackingTeam;
        this.defendingTeam = defendingTeam;
        this.ball = ball;
        this.config = config;

        RuckPosition = carrier.transform.position;

        attackingStack = new RuckStack(RuckPosition, -GetAttackDirection(), config.stackOffset);
        defendingStack = new RuckStack(RuckPosition, GetAttackDirection(), config.stackOffset);
    }
    public void StartRuck()
    {
        Phase = RuckPhase.Forming;

        carrier.SetControlled(false);
        ball.SetRuckLock(true);

        DropBallAtRuckPosition();
        BindCarrierToRuck();
        BindTacklerToRuck();
        SendSupportPlayers();
        AssignScrumhalf();
    }

    public void Tick(float deltaTime)
    {
        if (Phase == RuckPhase.Unloading)
        {
            TickUnloading(deltaTime);
        }
    }
    public void NotifySupportArrived(Player player, Team team)
    {
        if (IsAttackingTeam(team))
        {
            attackingStack.Push(player);
            attackingSupportArrived++;
        }
        else
        {
            defendingStack.Push(player);
            defendingSupportArrived++;
        }

        if (HasAllSupportArrived())
        {
            WinRuck();
        }
    }
    public void NotifyScrumhalfArrived()
    {
        Phase = RuckPhase.PlayingBall;
        GiveBallToScrumhalf();
        SwitchHumanControlToScrumhalf();
    }

    public void NotifyPassMade()
    {
        BeginUnloading();
    }

    private void DropBallAtRuckPosition()
    {
        Debug.LogAssertion("Dropping ball");
        ball.Drop();
        ball.transform.position = RuckPosition;
    }

    private void BindCarrierToRuck()
    {
        attackingStack.Push(carrier);
        EnterRuckBoundState(carrier);
        carrier.SetControlled(false);
    }

    private void BindTacklerToRuck()
    {
        defendingStack.Push(tackler);
        EnterRuckBoundState(tackler);
    }
    private void SendSupportPlayers()
    {
        List<Player> attackingSupport = FindSupportPlayers(attackingTeam, config.supportPlayersPerTeam, excludeScrumhalf: true);
        List<Player> defendingSupport = FindSupportPlayers(defendingTeam, config.supportPlayersPerTeam, excludeScrumhalf: false);

        attackingSupportExpected = attackingSupport.Count;
        defendingSupportExpected = defendingSupport.Count;

        SendTeamToRuck(attackingSupport, attackingTeam);
        SendTeamToRuck(defendingSupport, defendingTeam);
    }
    private void AssignScrumhalf()
    {
        scrumhalf = FindScrumhalf();
        if (scrumhalf == null) return;

        Vector3 ScrumhalfPosition = CalculateScrumhalfPosition();
        scrumhalf.stateMachine.SM.SetState(new RuckJoinState(scrumhalf.stateMachine, this, ScrumhalfPosition, isScrumhalf: true));
    }

    private void WinRuck()
    {
        Phase = RuckPhase.Won;

        if (scrumhalf == null)
        {
            BeginUnloading();
        }
    }
    private void GiveBallToScrumhalf()
    {
        ball.AttachTo(scrumhalf.transform);
    }
    private void SwitchHumanControlToScrumhalf()
    {
        scrumhalf.SetControlled(true);
        scrumhalf.stateMachine.SM.SetState(new RuckPlayState(scrumhalf.stateMachine, this));
    }
    private void BeginUnloading()
    {
        Phase = RuckPhase.Unloading;
        attackingStack.BeginUnloading(config.unloadInterval);
        defendingStack.BeginUnloading(config.unloadInterval);
    }

    private void TickUnloading(float deltaTime)
    {
        attackingStack.Tick(deltaTime);
        defendingStack.Tick(deltaTime);

        if (IsUnloadingComplete())
        {
            CompleteRuck();
        }
    }
    private bool IsUnloadingComplete()
    {
        return attackingStack.IsEmpty() && defendingStack.IsEmpty();
    }

    private void CompleteRuck()
    {
        Phase = RuckPhase.Forming; // reset
        ball.SetRuckLock(false);
        OnRuckComplete?.Invoke();
    }

    private void EnterRuckBoundState(Player player)
    {
        player.stateMachine.SM.SetState(new RuckBoundState(player.stateMachine));
    }
    private void SendTeamToRuck(List<Player> players, Team team)
    {
        foreach (Player player in players)
        {
            Vector3 stackPosition = CalculateStackPosition(team, players.IndexOf(player));
            player.stateMachine.SM.SetState(new RuckJoinState(player.stateMachine, this, stackPosition, isScrumhalf: false));
        }
    }
    private List<Player> FindSupportPlayers(Team team, int count, bool excludeScrumhalf)
    {
        List<Player> candidates = BuildCandidateList(team, excludeScrumhalf);
        SortByDistanceToRuck(candidates);
        return TakeFirst(candidates, count);
    }

    private List<Player> BuildCandidateList(Team team, bool excludeScrumhalf)
    {
        List<Player> candidates = new List<Player>();

        foreach (Player player in team.players)
        {
            if (!IsValidSupportCandidate(player, excludeScrumhalf)) continue;
            candidates.Add(player);
        }

        return candidates;
    }
    private bool IsValidSupportCandidate(Player player, bool excludeScrumhalf)
    {
        if (player == null) return false;
        if (player == carrier) return false;
        if (player == tackler) return false;
        if (excludeScrumhalf && player == scrumhalf) return false;
        return true;
    }
    private void SortByDistanceToRuck(List<Player> players)
    {
        players.Sort(CompareDistanceToRuck);
    }

    private int CompareDistanceToRuck(Player a, Player b)
    {
        float distA = DistanceToRuck(a);
        float distB = DistanceToRuck(b);
        return distA.CompareTo(distB);
    }
    private float DistanceToRuck(Player player)
    {
        return Vector3.Distance(player.transform.position, RuckPosition);
    }

    private List<Player> TakeFirst(List<Player> players, int count)
    {
        List<Player> result = new List<Player>();
        for (int i = 0; i < Mathf.Min(count, players.Count); i++)
        {
            result.Add(players[i]);
        }
        return result;
    }
    private Player FindScrumhalf()
    {
        List<Player> candidates = BuildCandidateList(attackingTeam, excludeScrumhalf: false);
        SortByDistanceToRuck(candidates);

        // Scrumhalf is closest attacker not joining the ruck as support
        int supportCount = config.supportPlayersPerTeam;
        if (candidates.Count > supportCount)
        {
            return candidates[supportCount];
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[candidates.Count - 1];
    }
    private Vector3 CalculateStackPosition(Team team, int slotIndex)
    {
        Vector3 stackDirection;

        if (IsAttackingTeam(team))
        {
            stackDirection = -GetAttackDirection();
        }
        else
        {
            stackDirection = GetAttackDirection();
        }
        float depth = config.stackSpacing * (slotIndex + 1);
        float lateralOffset = CalculateLateralStackOffset(slotIndex);

        Vector3 position = RuckPosition + stackDirection * depth;
        position += GetLateralDirection() * lateralOffset;
        return position;
    }

    private float CalculateLateralStackOffset(int slotIndex)
    {
        // Alternate left/right so players are slightly offset and visible
        float side;

        if (slotIndex % 2 == 0)
        {
            side = 1f;
        }
        else
        {
            side = -1f;
        }
        return side * config.stackOffset;
    }
    private Vector3 CalculateScrumhalfPosition()
    {
        return RuckPosition + (-GetAttackDirection() * config.scrumhalfDistance);
    }

    private bool HasAllSupportArrived()
    {
        return attackingSupportArrived >= attackingSupportExpected && defendingSupportArrived >= defendingSupportExpected;
    }
    private bool IsAttackingTeam(Team team)
    {
        return team == attackingTeam;
    }

    private Vector3 GetAttackDirection()
    {
        Vector3 dir = attackingTeam.attackDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        return dir.normalized;
    }

    private Vector3 GetLateralDirection()
    {
        return Vector3.Cross(Vector3.up, GetAttackDirection()).normalized;
    }

}
