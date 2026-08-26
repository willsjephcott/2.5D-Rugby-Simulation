using UnityEngine;
using System.Collections.Generic;

public class AttackingFormationController : MonoBehaviour
{
    public Team team;

    [Header("Formation Settings")] //adds a header in the inspector so i can edit this stuff
    public int numberOfLanes = 4;
    public float firstLaneDepth = 3f;
    public float depthIncrement = 2.5f;
    public float forwardBaseWidth = 3.5f;
    public float backBaseWidth = 4.5f;
    public float widthIncrement = 2.5f;
    public float formationFollowSpeed = 4f;
    public float arrivalThreshold = 0.3f;
    public bool forwardsInsideLanes = true;

    public bool drawGizmos = true;

    FormationPlanner planner;
    Transform currentCarrier;
    Dictionary<Player, Vector3> currentAssignments;

    private void Awake()
    {
        InitialiseSettings();
        InitialiseAssignments();
    }
    private void Start()
    {
        SubscribeToBallEvents();
    }
    private void OnDestroy()
    {
        UnsubscribeFromBallEvents();
    }
    private void FixedUpdate()
    {
        UpdateFormation();
    }
    private void InitialiseSettings()
    {
        FormationSettings settings = BuildSettingsFromFields();
        planner = new FormationPlanner(settings);
    }
    private FormationSettings BuildSettingsFromFields()
    {
        FormationSettings settings = new FormationSettings();
        settings.numberOfLanes = numberOfLanes;
        settings.firstLaneDepth = firstLaneDepth;
        settings.depthIncrement = depthIncrement;
        settings.forwardBaseWidth = forwardBaseWidth;
        settings.backBaseWidth = backBaseWidth;
        settings.widthIncrement = widthIncrement;
        settings.formationFollowSpeed = formationFollowSpeed;
        settings.arrivalThreshold = arrivalThreshold;
        settings.forwardsInsideLanes = forwardsInsideLanes;
        return settings;
    }
    private void InitialiseAssignments()
    {
        currentAssignments = new Dictionary<Player, Vector3>();
    }
    private void UpdateFormation()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.IsConversionActive()) return;
        if (MatchManager.Instance != null && MatchManager.Instance.IsLineoutActive()) return;

        if (!ValidateCanUpdateFormation())
        {
            ClearAllFormationTargets();
            return;
        }

        Transform carrier = GetBallCarrier();

        if (carrier == null)
        {
            ClearAllFormationTargets();
            return;
        }

        if (HasCarrierChanged(carrier))
        {
            OnCarrierChanged(carrier);
        }

        List<Player> availablePlayers = GetAvailablePlayers();

        if (availablePlayers.Count == 0)
        {
            ClearAllFormationTargets();
            return;
        }

        CalculateAndAssignFormation(carrier, availablePlayers);
    }
    private void CalculateAndAssignFormation(Transform carrier, List<Player> availablePlayers)
    {
        Dictionary<Player, Vector3> newAssignments = planner.CalculateFormation(carrier, availablePlayers, team.attackDirection);
        ApplyAssignments(newAssignments);
        ClearUnassignedPlayers(newAssignments);
        currentAssignments = newAssignments;
    }
    private void ApplyAssignments(Dictionary<Player, Vector3> assignments)
    {
        foreach (var assignment in assignments)
        {
            Player player = assignment.Key;
            Vector3 targetPosition = assignment.Value;

            if (!ShouldControlPlayer(player)) continue;

            EnsurePlayerInFormationState(player);
            GetFormationState(player)?.UpdateTarget(targetPosition);
        }
    }
    private bool ShouldControlPlayer(Player player)
    {
        if (player == null) return false;
        if (player.isControlled) return false;
        if (IsPlayerCarrier(player)) return false;
        if (IsPlayerInRuck(player)) return false;
        if (IsPlayerInLineout(player)) return false;
        return true;
    }
    private bool IsPlayerInRuck(Player player)
    {
        var current = player.stateMachine.SM.CurrentState;
        return current is RuckBoundState || current is RuckJoinState || current is RuckPlayState;
    }
    private bool IsPlayerInLineout(Player player)
    {
        var current = player.stateMachine.SM.CurrentState;
        return current is LineoutJoinState || current is LineoutBoundState;
    }
    private void EnsurePlayerInFormationState(Player player)
    {
        if (player.isControlled) return;
        if (IsPlayerCarrier(player)) return;

        if (!(player.stateMachine.SM.CurrentState is AIFormationState))
        {
            player.stateMachine.SM.SetState(new AIFormationState(player.stateMachine));
        }
    }
    private AIFormationState GetFormationState(Player player)
    {
        return player.stateMachine.SM.CurrentState as AIFormationState;
    }
    private void ClearUnassignedPlayers(Dictionary<Player, Vector3> assignments)
    {
        foreach (Player player in team.players)
        {
            if (!ShouldClearPlayer(player, assignments)) continue;

            if (player.stateMachine.SM.CurrentState is AIFormationState)
            {
                player.stateMachine.SM.SetState(new AIIdleState(player.stateMachine));
            }
        }
    }
    private bool ShouldClearPlayer(Player player, Dictionary<Player, Vector3> assignments)
    {
        if (player == null) return false;
        if (player.isControlled) return false;
        if (IsPlayerCarrier(player)) return false;
        if (IsPlayerInRuck(player)) return false;
        if (assignments.ContainsKey(player)) return false;
        return true;
    }
    private List<Player> GetAvailablePlayers()
    {
        List<Player> available = new List<Player>();
        if (team?.players == null) return available;

        foreach (Player player in team.players)
        {
            if (IsPlayerAvailableForFormation(player)) available.Add(player);
        }

        return available;
    }
    private bool IsPlayerCarrier(Player player)
    {
        if (currentCarrier == null) return false;
        return player.transform == currentCarrier;
    }
    private bool IsPlayerAvailableForFormation(Player player)
    {
        if (player == null) return false;
        if (IsPlayerCarrier(player)) return false;
        if (IsPlayerInRuck(player)) return false;
        if (IsPlayerInLineout(player)) return false;
        return true;
    }
    private Transform GetBallCarrier()
    {
        Ball ball = GetBall();
        if (ball == null) return null;

        // Ball is held normally
        if (ball.currentHolder != null) return ball.currentHolder;

        // During a ruck, anchor formation around the scrumhalf
        if (MatchManager.Instance != null && MatchManager.Instance.IsRuckActive())
        {
            RuckManager ruck = MatchManager.Instance.GetActiveRuck();
            Player scrumhalf = ruck?.GetScrumhalf();
            if (scrumhalf != null) return scrumhalf.transform;

            // Scrumhalf not assigned yet — use ruck position via a dummy isn't possible,
            // so return null and let formation clear until scrumhalf is assigned
            return null;
        }

        return null;
    }
    private bool HasCarrierChanged(Transform carrier)
    {
        return carrier != currentCarrier;
    }
    private void OnCarrierChanged(Transform newCarrier)
    {
        currentCarrier = newCarrier;
        DebugLogCarrierChange(newCarrier);
    }
    private void ClearAllFormationTargets()
    {
        if (team?.players == null) return;

        foreach (Player player in team.players)
        {
            ClearFormationTargetForPlayer(player);
        }

        currentAssignments?.Clear();
    }
    private void ClearFormationTargetForPlayer(Player player)
    {
        if (player == null) return;

        AIFormationState formationState = GetFormationState(player);
        formationState?.ClearTarget();
    }
    private void SubscribeToBallEvents()
    {
        Ball ball = GetBall();
        if (ball != null)
        {
            ball.OwnerChanged -= OnBallOwnerChanged;
            ball.OwnerChanged += OnBallOwnerChanged;
        }
    }
    private void UnsubscribeFromBallEvents()
    {
        Ball ball = GetBall();
        if (ball != null)
        {
            ball.OwnerChanged -= OnBallOwnerChanged;
        }
    }
    private void OnBallOwnerChanged(Transform oldHolder, Transform newHolder)
    {
        // Formation updates automatically next FixedUpdate
    }
    private bool ValidateCanUpdateFormation()
    {
        return team != null;
    }
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }
    private void DebugLogCarrierChange(Transform newCarrier)
    {
        string name = newCarrier != null ? newCarrier.name : "none";
        Debug.Log($"AttackingFormationController: carrier changed to {name}.");
    }
    private void DebugLogCreatedDefaultSettings()
    {
        Debug.Log("AttackingFormationController: using Inspector formation settings.");
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (team?.players == null) return;

        // Draw attacking formation slot targets
        foreach (Player player in team.players)
        {
            if (player == null) continue;

            AIFormationState formationState = player.stateMachine?.SM?.CurrentState as AIFormationState;
            if (formationState != null)
            {
                formationState.DebugDrawTarget(player.transform.position);
            }
        }

        // Draw the current assignment targets directly from the planner output
        if (currentAssignments == null) return;
        foreach (var kvp in currentAssignments)
        {
            if (kvp.Key == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(kvp.Value, Vector3.one * 0.4f);
        }
    }
}