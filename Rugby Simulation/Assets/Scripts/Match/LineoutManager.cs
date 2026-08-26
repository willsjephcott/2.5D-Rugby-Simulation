using System.Collections.Generic;
using UnityEngine;
using System;

public class LineoutManager : IContest
{
    public enum LineoutPhase
    {
        Forming,        // Players running to slots
        SelectingSlot,  // Human picks Front/Middle/Back
        Contesting,     // Needle QTE active
        Complete        // Lineout resolved, players released
    }

    public LineoutPhase Phase { get; private set; }
    public Vector3 LineoutPosition { get; private set; }

    Team throwingTeam;
    Team defendingTeam;
    Ball ball;
    LineoutConfig config;

    LineoutJumpQueue throwingQueue = new LineoutJumpQueue();
    LineoutJumpQueue defendingQueue = new LineoutJumpQueue();

    int throwingArrived;
    int defendingArrived;
    int throwingExpected;
    int defendingExpected;

    // Slot index 0=Front 1=Middle 2=Back 
    int selectedSlotIndex = -1;

    public event Action OnLineoutComplete;
    public event Action OnFormingComplete;  // Triggers UI to show slot selection

    public LineoutManager(Team throwingTeam, Team defendingTeam, Ball ball, LineoutConfig config, Vector3 lineoutPosition)
    {
        this.throwingTeam = throwingTeam;
        this.defendingTeam = defendingTeam;
        this.ball = ball;
        this.config = config;
        this.lineoutPosition = lineoutPosition;
        LineoutPosition = lineoutPosition;
    }

    Vector3 lineoutPosition;

    public void StartLineout()
    {
        Phase = LineoutPhase.Forming;
        ball.SetLineoutLock(true);
        SendPlayersToSlots();
    }

    // Called by LineoutNeedleUI when human selects Front/Middle/Back
    public void NotifySlotSelected(int slotIndex)
    {
        if (Phase != LineoutPhase.SelectingSlot) return;
        selectedSlotIndex = slotIndex;
        Phase = LineoutPhase.Contesting;
    }
    // Called by LineoutNeedleUI when the QTE resolves
    public void NotifyContestResult(bool throwingTeamWon)
    {
        if (Phase != LineoutPhase.Contesting) return;

        Player winner = DetermineWinner(throwingTeamWon);
        ReleaseAllPlayers();
        CompleteLineout();
        GiveBallToWinner(winner);
    }
    // Called by LineoutJoinState when a player arrives at their slot
    public void NotifyPlayerArrived(Player player, bool isAttacking)
    {
        if (isAttacking) throwingArrived++;
        else defendingArrived++;

        //Debug.Log($"Player arrived: {player.name}, isAttacking={isAttacking}, throwingArrived={throwingArrived}/{throwingExpected}, defendingArrived={defendingArrived}/{defendingExpected}");


        if (HasAllPlayersArrived())
        {
            //Debug.Log("All players arrived - firing OnFormingComplete");
            Phase = LineoutPhase.SelectingSlot;
            OnFormingComplete?.Invoke();
        }
    }
    public Vector3 GetSlotPosition(int slotIndex, bool isAttacking)
    {
        return CalculateSlotPosition(slotIndex, isAttacking);
    }

    public int GetPlayerCount()
    {
        return config.playersPerTeam;
    }


    private void SendPlayersToSlots()
    {
        List<Player> throwingForwards = FindClosestForwards(throwingTeam);
        List<Player> defendingForwards = FindClosestForwards(defendingTeam);

        throwingExpected = Mathf.Min(throwingForwards.Count, config.playersPerTeam);
        defendingExpected = Mathf.Min(defendingForwards.Count, config.playersPerTeam);

        SendTeamToSlots(throwingForwards, isAttacking: true);
        SendTeamToSlots(defendingForwards, isAttacking: false);
    }
    private void SendTeamToSlots(List<Player> players, bool isAttacking)
    {
        int count = Mathf.Min(players.Count, config.playersPerTeam);
        Debug.Log($"SendTeamToSlots: isAttacking={isAttacking}, count={count}");
        LineoutJumpQueue queue = isAttacking ? throwingQueue : defendingQueue;

        for (int i = 0; i < count; i++)
        {
            Player player = players[i];
            Vector3 slotPos = CalculateSlotPosition(i, isAttacking);
            queue.Enqueue(player, slotPos);
            player.rb.MovePosition(slotPos);
            player.stateMachine.SM.SetState(new LineoutBoundState(player.stateMachine));
            NotifyPlayerArrived(player, isAttacking);

        }
    }
    //Dont think this works yet need to test more (maybe add some gizmos)
    private Vector3 CalculateSlotPosition(int slotIndex, bool isAttacking)
    {
        // Players line up along Z axis (perpendicular to touchline)
        // Throwing team on one side of lineout position X, defending on the other
        float sideOffset;

        if (isAttacking)
        {
            sideOffset = -config.teamGap;
        }
        else
        {
            sideOffset= config.teamGap;
        }
            float zOffset = slotIndex * config.slotSpacing;

        return new Vector3(LineoutPosition.x + sideOffset,LineoutPosition.y,LineoutPosition.z + zOffset);
    }
    private Player DetermineWinner(bool throwingTeamWon)
    {
        if (throwingTeamWon)
        {
            // Ball goes to selected slot player on throwing team
            return throwingQueue.GetPlayer(selectedSlotIndex);
        }
        else
        {
            // Defending team wins — give to the player in the contested slot
            return defendingQueue.GetPlayer(selectedSlotIndex);
        }
    }
    private void GiveBallToWinner(Player winner)
    {
        if (winner == null)
        {
            DebugNullLineout();
            ball.SetLineoutLock(false);
            return;
        }

        // Reuse PassHandler arc throw to the winning player
        ball.passHandler.RequestPassToPlayer(winner.transform);
    }

    private void ReleaseAllPlayers()
    {
        ReleaseQueue(throwingQueue);
        ReleaseQueue(defendingQueue);
    }
    private void ReleaseQueue(LineoutJumpQueue queue)
    {
        while (queue.Dequeue(out Player player, out Vector3 _))
        {
            if (player == null) continue;
            player.stateMachine.SM.SetState(new AIIdleState(player.stateMachine));
        }
    }
    private void CompleteLineout()
    {
        Phase = LineoutPhase.Complete;
        ball.SetLineoutLock(false);    
        OnLineoutComplete?.Invoke();
    }

    private bool HasAllPlayersArrived()
    {
        return throwingArrived >= throwingExpected && defendingArrived >= defendingExpected && throwingExpected > 0 && defendingExpected > 0;
    }
    private List<Player> FindClosestForwards(Team team)
    {
        List<Player> forwards = GetForwards(team);
        SortByDistanceToLineout(forwards);
        return TakeFirst(forwards, config.playersPerTeam);
    }

    private List<Player> GetForwards(Team team)
    {
        List<Player> forwards = new List<Player>();
        if (team?.players == null) return forwards;

        foreach (Player player in team.players)
        {
            if (player != null && player.playerGroup == PlayerGroup.Forward) forwards.Add(player);
        }
        return forwards;
    }
    private void SortByDistanceToLineout(List<Player> players)
    {
        players.Sort(CompareDistanceToLineout);
    }
    private int CompareDistanceToLineout(Player a, Player b)
    {
        return DistanceToLineout(a).CompareTo(DistanceToLineout(b));
    }

    private float DistanceToLineout(Player player)
    {
        return Vector3.Distance(player.transform.position, LineoutPosition);
    }

    private List<Player> TakeFirst(List<Player> players, int count)
    {
        List<Player> result = new List<Player>();
        for (int i = 0; i < Mathf.Min(count, players.Count); i++) result.Add(players[i]);
        return result;
    }
    private void DebugNullLineout()
    {
        Debug.LogWarning("LineoutManager: Winner player is null, dropping ball.");
    }
}

