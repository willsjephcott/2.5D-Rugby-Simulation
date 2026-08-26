using UnityEngine;
using System.Collections.Generic;

public class DefensiveLineController : MonoBehaviour
{
    public Team team;
    public Team opposingTeam;
    public bool drawGizmos = true;

    Dictionary<Player, Player> opponentAssignments;

    List<Player> lastDefenders = new List<Player>();
    List<Player> lastOpponents = new List<Player>();
    bool assignmentsDirty = true; // something has changed recalculate

    private void Awake()
    {
        InitialiseAssignments();
    }
    private void OnEnable()
    {
        MarkAssignmentsDirty();
    }
    private void FixedUpdate()
    {
        UpdateDefensiveLine();
    }
    private void InitialiseAssignments()
    {
        opponentAssignments = new Dictionary<Player, Player>();
    }
    private void MarkAssignmentsDirty()
    {
        assignmentsDirty = true;
    }
    private void UpdateDefensiveLine()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.IsConversionActive()) return;
        if (MatchManager.Instance != null && MatchManager.Instance.IsLineoutActive()) return; 
        if (!ValidateTeams()) return;

        List<Player> defenders = GetAvailableDefenders();
        List<Player> opponents = GetAvailableOpponents();

        if (!HasEnoughPlayersForLine(defenders, opponents))
        {
            ClearAllDefensiveStates();
            return;
        }

        if (ShouldRebuildAssignments(defenders, opponents))
        {
            RebuildAssignments(defenders, opponents);
        }

        ApplyDefensiveStates(defenders);
    }
    private bool HasEnoughPlayersForLine(List<Player> defenders, List<Player> opponents)
    {
        return defenders.Count > 0 && opponents.Count > 0;
    }
    private bool ShouldRebuildAssignments(List<Player> defenders, List<Player> opponents)
    {
        return assignmentsDirty || HaveDefendersChanged(defenders) || HaveOpponentsChanged(opponents);
    }
    private void RebuildAssignments(List<Player> defenders, List<Player> opponents)
    {
        AssignOpponentsToDefenders(defenders, opponents);
        CachePlayerLists(defenders, opponents);
        assignmentsDirty = false;
    }
    private bool HaveDefendersChanged(List<Player> defenders)
    {
        return !PlayerListMatches(defenders, lastDefenders);
    }
    private bool HaveOpponentsChanged(List<Player> opponents)
    {
        return !PlayerListMatches(opponents, lastOpponents);
    }
    private bool PlayerListMatches(List<Player> a, List<Player> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
    private void CachePlayerLists(List<Player> defenders, List<Player> opponents)
    {
        lastDefenders = new List<Player>(defenders);
        lastOpponents = new List<Player>(opponents);
    }
    private void AssignOpponentsToDefenders(List<Player> defenders, List<Player> opponents)
    {
        opponentAssignments.Clear();

        SortByXPosition(defenders);
        SortByXPosition(opponents);

        int pairCount = Mathf.Min(defenders.Count, opponents.Count);

        AssignPairedDefenders(defenders, opponents, pairCount);
        ClearUnpairedDefenders(defenders, pairCount);
    }
    private void SortByXPosition(List<Player> players)
    {
        players.Sort(CompareByX);
    }
    private int CompareByX(Player a, Player b)
    {
        return a.transform.position.x.CompareTo(b.transform.position.x);
    }
    private void AssignPairedDefenders(List<Player> defenders, List<Player> opponents, int pairCount)
    {
        for (int i = 0; i < pairCount; i++)
        {
            opponentAssignments[defenders[i]] = opponents[i];
        }
    }
    private void ClearUnpairedDefenders(List<Player> defenders, int pairCount)
    {
        for (int i = pairCount; i < defenders.Count; i++)
        {
            ClearDefenderState(defenders[i]);
        }
    }
    private void ApplyDefensiveStates(List<Player> defenders)
    {
        List<Player> allDefenders = new List<Player>(defenders);

        foreach (Player defender in defenders)
        {
            TryApplyDefensiveState(defender, allDefenders);
        }
    }
    private void TryApplyDefensiveState(Player defender, List<Player> allDefenders)
    {
        if (!opponentAssignments.ContainsKey(defender)) return;
        if (!ShouldControlPlayer(defender)) return;
        if (IsPlayerInLineout(defender)) return;

        Player assignedOpponent = opponentAssignments[defender];
        EnsurePlayerInDefensiveLineState(defender, assignedOpponent, allDefenders);
    }
    private void EnsurePlayerInDefensiveLineState(Player defender, Player opponent, List<Player> allDefenders)
    {
        if (IsInHigherPriorityState(defender)) return;

        if (IsInDefensiveLineState(defender))
        {
            UpdateExistingDefensiveLineState(defender, opponent, allDefenders);
        }
        else
        {
            EnterDefensiveLineState(defender, opponent, allDefenders);
        }
    }
    private bool IsInHigherPriorityState(Player defender)
    {
        return IsInChaseOrTackle(defender) || IsInRuckState(defender);
    }
    private bool IsInChaseOrTackle(Player defender)
    {
        var current = defender.stateMachine.SM.CurrentState;
        return current is ChaseState || current is TackleState;
    }
    private bool IsInRuckState(Player defender)
    {
        var current = defender.stateMachine.SM.CurrentState;
        return current is RuckBoundState || current is RuckJoinState || current is RuckPlayState;
    }
    private bool IsInDefensiveLineState(Player defender)
    {
        return defender.stateMachine.SM.CurrentState is DefensiveLineState;
    }
    private void UpdateExistingDefensiveLineState(Player defender, Player opponent, List<Player> allDefenders)
    {
        var lineState = defender.stateMachine.SM.CurrentState as DefensiveLineState;
        lineState?.UpdateAssignment(opponent, allDefenders);
    }
    private void EnterDefensiveLineState(Player defender, Player opponent, List<Player> allDefenders)
    {
        defender.stateMachine.SM.SetState(new DefensiveLineState(defender.stateMachine, opponent, allDefenders));
        DebugLogAssignment(defender, opponent);
    }
    private void ClearDefenderState(Player defender)
    {
        if (!CanClearDefenderState(defender)) return;
        if (IsPlayerInLineout(defender)) return;

        if (IsInDefensiveLineState(defender))
        {
            defender.stateMachine.SM.SetState(new AIIdleState(defender.stateMachine));
        }
    }
    private bool CanClearDefenderState(Player defender)
    {
        return defender != null && !defender.isControlled;
    }
    private void ClearAllDefensiveStates()
    {
        if (team?.players == null) return;

        foreach (Player defender in team.players)
        {
            ClearDefenderState(defender);
        }

        opponentAssignments.Clear();
        MarkAssignmentsDirty();
    }
    private List<Player> GetAvailableDefenders()
    {
        List<Player> available = new List<Player>();
        if (team?.players == null) return available;

        foreach (Player p in team.players)
        {
            if (IsAvailableForDefensiveLine(p)) available.Add(p);
        }

        return available;
    }
    private List<Player> GetAvailableOpponents()
    {
        List<Player> available = new List<Player>();
        if (opposingTeam?.players == null) return available;

        foreach (Player p in opposingTeam.players)
        {
            if (p != null) available.Add(p);
        }

        return available;
    }
    private bool IsAvailableForDefensiveLine(Player player)
    {
        if (player == null) return false;
        if (player.isControlled) return false;
        if (IsPlayerInLineout(player)) return false;
        return true;
    }
    private bool ShouldControlPlayer(Player player)
    {
        if (player == null) return false;
        if (player.isControlled) return false;
        if (IsPlayerInLineout(player)) return false;
        return true;
    }
    private bool IsPlayerInLineout(Player player)
    {
        var current = player.stateMachine.SM.CurrentState;
        return current is LineoutJoinState || current is LineoutBoundState;
    }
    private bool ValidateTeams()
    {
        if (team == null)
        {
            DebugLogMissingTeam("team");
            return false;
        }
        if (opposingTeam == null)
        {
            DebugLogMissingTeam("opposingTeam");
            return false;
        }
        return true;
    }
    private void DebugLogAssignment(Player defender, Player opponent)
    {
        //Debug.Log($"{defender.name} assigned to mark {opponent.name}.");
    }
    private void DebugLogMissingTeam(string fieldName)
    {
        Debug.LogWarning($"DefensiveLineController: {fieldName} is not assigned.");
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (team?.players == null) return;

        foreach (Player defender in team.players)
        {
            if (defender == null) continue;

            DefensiveLineState lineState = defender.stateMachine?.SM?.CurrentState as DefensiveLineState;
            if (lineState == null) continue;

            // Cyan cube at the slot target
            Vector3 target = lineState.GetDebugTarget();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(target, Vector3.one * 0.4f);

            // Line from defender to slot target
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawLine(defender.transform.position, target);

            // Red line to assigned opponent + sphere on them
            Player opponent = lineState.GetDebugOpponent();
            if (opponent != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(defender.transform.position, opponent.transform.position);
                Gizmos.DrawWireSphere(opponent.transform.position, 0.25f);
            }
        }
    }
}

/*using System.Collections.Generic;
using UnityEngine;

public class DefensiveLineController : MonoBehaviour
{
    public Team team;
    public Ball ball;
    public GameConfig config;

    public Vector3 defenceDirection = Vector3.back;

    public bool enableDebug = true;
    public bool enableGizoms = true;


    DefensiveLineSolver = solver = new List<DefenderAI>();
    List<DefenderAI> allDefenders = new List<DefenderAI>();
    List<DefenderAI> availableDefenders = new List<DefenderAI>();

    float passReactionTimer;

    Transform lastKnownBallHolder;

    private void Awake()
    {
        InitialiseSolver();
    }
    private void Start()
    {
        CollectDefenderAI();
        SubscribeToBallEvents();
    }
    private void OnDestroy()
    {
        UnsubscribeFromBallEvents();
    }

    private void FixedUpdate()
    {
        TickPassReactionTimer();
        GatherAvailableAI();

        DefensiveLineSolver.BallPose pose = BuildBallPose();

        HandleLooseBallChaser(pose);
        EvaluateChaseConditions(pose);
        SolveAndDistributeSlots(pose);
        MoveAllAI(pose);
    }
    private void InitialiseSolver()
    {
        if (config == null)
        {
            DebugLogMissingConfig();
            return;
        }
        solver = new DefensiveLineSolver(config);
    }
    public void CollectDefenderAI()
    {
        allDefenders.Clear();
        if (team == null) return;

        foreach (Player p in team.players)
        {
            if (p == null) continue;

            DefenderAI AI = p.GetComponent<DefenderAI>();
            if (AI == null)
            {
                AI = p.gameObject.AddComponent<DefenderAI>();
            }
            AI.Initialise(p, config);
            allDefenders.Add(AI);
        }
    }
    private void SubscribeToBallEvents()
    {
        if (ball == null)
        {
            ball = FindBall();
        }
        if (ball != null)
        {
            ball.OwnerChanged -= OnBallOwnerChanged;
            ball.OwnerChanged += OnBallOwnerChanged;
        }
    }

    private void UnsubscribeFromBallEvents()
    {
        if (ball != null)
        {
            ball.OwnerChanged -= OnBallOwnerChanged;
        }
    }
    private void OnBallOwnerChanged(Transform oldHolder, Transform newHolder)
    {
        StartPassReactionCooldown();
        lastKnownBallHolder = newHolder;

        // If possession flipped to OUR team, disengage all chasers
        if (newHolder != null && IsOnOurTeam(newHolder))
        {
            DisengageAllChasers();
        }

        DebugLogPassReaction(oldHolder, newHolder);
    }
    private bool IsOnOurTeam(Transform holder)
    {
        Player p = holder.GetComponentInParent<Player>();
        return p != null && p.team == team;
    }
    private void StartPassReactionCooldown()
    {
        if (config != null)
            passReactionTimer = config.passReactionCooldown;
    }

    private void TickPassReactionTimer()
    {
        if (passReactionTimer > 0f)
            passReactionTimer -= Time.fixedDeltaTime;
    }

    private bool IsInPassReaction()
    {
        return passReactionTimer > 0f;
    }
    private void GatherAvailableDefenderd()
    {
        availableDefenders.Clear();
        for (int i = 0; i < allDefenders.Count; i++)
        {
            DefenderAI AI = allDefenders[i];
            if (AI == null) continue;
            if (AI.IsAvailableForLine())
            {
                availableDefenders.Add(AI);
            }
        }
    }
    private void EvaluateChaseConditions(DefensiveLineSolver.BallPose pose)
    {
        if (config == null) return;
        if (!pose.isHeld) return; // loose-ball chase handled separately

        CleanUpInvalidChasers();

        // Don't assign new chasers during pass-reaction cooldown (anti-jitter)
        if (IsInPassReaction()) return;

        Vector3 ballPos = pose.position;

        // Try to fill up to maxChasers
        if (activeChasers.Count >= config.maxChasers) return;

        DefenderAI bestCandidate = FindBestChaseCandidate(ballPos);
        if (bestCandidate != null)
        {
            ActivateChaser(bestCandidate);
        }
    }
    private DefenderAI FindBestChaseCandidate(Vector3 ballPos)
    {
        DefenderAI best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < availableDefenders.Count; i++)
        {
            DefenderAI AI = availableDefenders[i];
            if (AI.IsChasing) continue;

            float dist = HorizontalDistance(AI.GetPosition(), ballPos);
            if (dist > config.chaseActivationDistance) continue;
            if (!IsWithinChaseAngle(AI, ballPos)) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = AI;
            }
        }

        return best;
    }
    private bool IsWithinChaseAngle(DefenderAI AI, Vector3 ballPos)
    {
        Vector3 toBall = ballPos - AI.GetPosition();
        toBall.y = 0f;
        if (toBall.sqrMagnitude < 0.01f) return true;

        float angle = Vector3.Angle(defenceDirection, toBall);
        return angle <= config.chaseAngleThreshold * 0.5f;
    }

    private void ActivateChaser(DefenderAI AI)
    {
        AI.BeginChase();
        activeChasers.Add(AI);

        // Transition the player's FSM to chase state
        TransitionToChaseState(AI);

        DebugLogChaseActivated(AI);
    }
    private void CleanUpInvalidChasers()
    {
        for (int i = activeChasers.Count - 1; i >= 0; i--)
        {
            DefenderAI chaser = activeChasers[i];
            if (chaser == null || !chaser.IsPhysicallyAvailable())
            {
                RemoveChaser(i);
                continue;
            }

            // Disengage if too far from ball
            float dist = HorizontalDistance(chaser.GetPosition(), GetBallPosition());
            if (dist > config.chaseDisengageDistance)
            {
                DisengageChaser(i);
            }
        }
    }
    private void DisengageChaser(int index)
    {
        DefenderAI AI = activeChasers[index];
        AI.EndChase();
        activeChasers.RemoveAt(index);

        // Return to defend-line state
        TransitionToDefendLineState(AI);

        DebugLogChaseDisengaged(AI);
    }

    private void RemoveChaser(int index)
    {
        if (activeChasers[index] != null)
            activeChasers[index].EndChase();
        activeChasers.RemoveAt(index);
    }
    private void DisengageAllChasers()
    {
        for (int i = activeChasers.Count - 1; i >= 0; i--)
        {
            DisengageChaser(i);
        }
    }
    private void HandleLooseBallChaser(DefensiveLineSolver.BallPose pose)
    {
        if (pose.isHeld) return;
        if (activeChasers.Count > 0) return; // already have someone going

        // Pick the nearest available defender to chase the loose ball
        DefenderAI nearest = FindNearestDefender(pose.position);
        if (nearest != null)
        {
            ActivateChaser(nearest);
        }
    }
    private DefenderAI FindNearestDefender(Vector3 target)
    {
        DefenderAI best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < availableDefenders.Count; i++)
        {
            float dist = HorizontalDistance(availableDefenders[i].GetPosition(), target);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = availableDefenders[i];
            }
        }
        return best;
    }
    private void SolveAndDistributeSlots(DefensiveLineSolver.BallPose pose)
    {
        if (solver == null) return;

        List<DefensiveLineSolver.SlotResult> results = solver.Solve(availableDefenders, pose, defenceDirection);

        for (int i = 0; i < results.Count; i++)
        {
            DefensiveLineSolver.SlotResult slot = results[i];
            slot.agent.SlotIndex = slot.slotIndex;
            slot.agent.SetSlotTarget(slot.targetPosition);
        }
    }
    private void MoveAllAgents(DefensiveLineSolver.BallPose pose)
    {
        // Line defenders: move toward their smoothed slot
        for (int i = 0; i < availableDefenders.Count; i++)
        {
            DefenderAI AI = availableDefenders[i];
            if (AI.IsChasing) continue;

            EnsureInDefendLineState(AI);
            AI.UpdateSlotMovement();
        }
        // Chasers: move toward the ball
        float chaseSpeed = config.maxDriftSpeed * config.chaseSpeedMultiplier;
        for (int i = 0; i < activeChasers.Count; i++)
        {
            activeChasers[i].MoveTowardPosition(pose.position, chaseSpeed);
        }
    }
    private void EnsureInDefendLineState(DefenderAI AI)
    {
        Player p = AI.GetPlayer();
        if (p == null || p.stateMachine == null) return;
        if (p.isControlled) return;

        // Only switch if not already in DefendLineState
        if (!(p.stateMachine.SM.CurrentState is DefendLineState))
        {
            p.stateMachine.SM.SetState(new DefendLineState(p.stateMachine, AI));
        }
    }

    private void TransitionToChaseState(DefenderAI AI)
    {
        Player p = AI.GetPlayer();
        if (p == null || p.stateMachine == null) return;

        p.stateMachine.SM.SetState(new ChaseState(p.stateMachine, AI, this));
    }
    private void TransitionToDefendLineState(DefenderAI AI)
    {
        Player p = AI.GetPlayer();
        if (p == null || p.stateMachine == null) return;

        p.stateMachine.SM.SetState(new DefendLineState(p.stateMachine, AI));
    }
    public void NotifyChaserFinished(DefenderAI AI)
    {
        int idx = activeChasers.IndexOf(AI);
        if (idx >= 0)
        {
            RemoveChaser(idx);
        }
    }

    public Vector3 GetBallPosition()
    {
        if (ball != null) return ball.transform.position;
        return Vector3.zero;
    }

    public Ball GetBall()
    {
        return ball;
    }
    private DefensiveLineSolver.BallPose BuildBallPose()
    {
        Vector3 pos = ball != null ? ball.transform.position : Vector3.zero;
        bool held = ball != null && ball.currentHolder != null;
        return new DefensiveLineSolver.BallPose
        {
            position = pos,
            isHeld = held,
            carrierVelocity = Vector3.zero // extend later if you track carrier rb
        };
    }

    private Ball FindBall()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.ball != null)
            return MatchManager.Instance.ball;
        return FindAnyObjectByType<Ball>();
    }

    private float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 diff = a - b;
        diff.y = 0f;
        return diff.magnitude;
    }

}*/