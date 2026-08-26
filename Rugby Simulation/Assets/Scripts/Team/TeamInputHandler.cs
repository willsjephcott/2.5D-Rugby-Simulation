using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Handles human input for a team (sends input to the controlled player)
// Listens for ball ownership changes to auto-switch the controls
public class TeamInputHandler : MonoBehaviour
{
    public Team team;
    public Team opposingTeam;
    public PassIndicatorUI passIndicatorUI;
    public int startPlayerIndex = 0;
    public bool isTeamA = true;

    int currentPlayerIndex;
    bool initialPlayerSet = false;

    GameConfig config;
    PassProbabilityAlgorithm probabilityAlgorithm;

    private void Awake()
    {
        Debug.Log($"TeamInputHandler Awake on {gameObject.name}");
        InitialiseConfig();
    }
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
        UnsubscribeFromBallEvents();
    }
    private void Start()
    {
        //Debug.Log($"TeamInputHandler Start on {gameObject.name}, team={team?.name}, playerCount={team?.players?.Count}");
        SetInitialControlledPlayer();
        SubscribeToBallEvents();
    }

    private void Update()
    {
        if (!ValidateTeamHasPlayers()) return;

        Player currentPlayer = GetCurrentPlayer();
        if (!ValidateCurrentPlayer(currentPlayer)) return;
        //Debug.Log($"{team.name} Update: isControlled={currentPlayer.isControlled}, state={currentPlayer.stateMachine.SM.CurrentState?.GetType().Name}");


        ProcessMovementInput(currentPlayer);
        ProcessPlayerSwitchingInput();
        ProcessPassInput(currentPlayer);
        ProcessTackleInput(currentPlayer);
        ProcessDefenderSwitchInput();
        UpdatePassIndicatorUI(currentPlayer);
    }
    
    private void InitialiseConfig()
    {
        config = FindConfig();

        if (!ValidateConfig())
        {
            DebugMissingConfig();
            return;
        }

        probabilityAlgorithm = config.CreateProbabilityAlgorithm();
    }
    private GameConfig FindConfig()
    {
        if (MatchManager.Instance != null)
        {
            return MatchManager.Instance.config;
        }
        return FindAnyObjectByType<GameConfig>();
    }
    private void SetInitialControlledPlayer()
    {
        if (!ValidateTeamHasPlayers()) return;
        int index = ClampPlayerIndex(startPlayerIndex);
        //Debug.Log($"{team.name} TeamInputHandler: SetInitialControlledPlayer index={index}");
        SetControlledPlayer(index);
        initialPlayerSet = true;
    }

    private void ProcessMovementInput(Player player)
    {
        Vector2 moveInput = ReadMoveInput();
        bool sprintInput = ReadSprintInput();
        SendInputToPlayer(player, moveInput, sprintInput);
    }
    private Vector2 ReadMoveInput()
    {
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        float x = 0f;
        float y = 0f;

        if (KeybindManager.IsKeyHeld(binds.moveLeft)) x = -1f;
        if (KeybindManager.IsKeyHeld(binds.moveRight)) x = 1f;
        if (KeybindManager.IsKeyHeld(binds.moveUp)) y = 1f;
        if (KeybindManager.IsKeyHeld(binds.moveDown)) y = -1f;

        return new Vector2(x, y);
    }
    private bool ReadSprintInput()
    {
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        return KeybindManager.IsKeyHeld(binds.sprint);
    }
    private void SendInputToPlayer(Player player, Vector2 move, bool sprint)
    {
        player.stateMachine.SetInput(move, sprint);
    }

    private void ProcessPlayerSwitchingInput()
    {
        // Player switching uses fixed keys — not remappable
        if (isTeamA)
        {
            if (Input.GetKeyDown(KeyCode.Tab)) SwitchToNextPlayer();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToNextPlayer();
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToPreviousPlayer();
    }
    private void SwitchToNextPlayer()
    {
        int nextIndex = (currentPlayerIndex + 1) % team.players.Count;
        SwitchToPlayerAtIndex(nextIndex);
    }
    private void SwitchToPreviousPlayer()
    {
        int prevIndex = (currentPlayerIndex - 1 + team.players.Count) % team.players.Count;
        SwitchToPlayerAtIndex(prevIndex);
    }
    private void SwitchToPlayerAtIndex(int index)
    {
        if (index == currentPlayerIndex) return;
        SetControlledPlayer(index);
    }
    private void SetControlledPlayer(int index)
    {
        DeactivateCurrentPlayer();
        currentPlayerIndex = index;
        ActivateNewPlayer();
        UpdateUIFollowTarget();
    }
    private void DeactivateCurrentPlayer()
    {
        if (!IsValidPlayerIndex(currentPlayerIndex)) return;
        team.players[currentPlayerIndex].SetControlled(false);
    }
    private void ActivateNewPlayer()
    {
        Player player = team.players[currentPlayerIndex];
        player.SetControlled(true);
        SetPlayerToCorrectHumanState(player);
    }
    private void ProcessDefenderSwitchInput()
    {
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        if (!KeybindManager.IsKeyDown(binds.switchDefender)) return;
        if (TeamIsAttacking()) return;

        SwitchToClosestDefender();
    }

    private void SwitchToClosestDefender()
    {
        Ball ball = GetBall();
        if (ball == null) return;

        Player best = FindClosestDefenderToBall(ball);
        if (best == null) return;

        HandOffCurrentPlayerToAI();
        SwitchControlToDefender(best);
    }

    private Player FindClosestDefenderToBall(Ball ball)
    {
        Player best = null;
        float bestDist = float.MaxValue;

        foreach (Player p in team.players)
        {
            if (p == null) continue;
            if (p.isControlled) continue;
            if (!(p.stateMachine?.SM?.CurrentState is DefensiveLineState)) continue;

            float dist = Vector3.Distance(p.transform.position, ball.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = p;
            }
        }

        return best;
    }

    private void HandOffCurrentPlayerToAI()
    {
        Player current = GetCurrentPlayer();
        if (current == null) return;

        current.SetControlled(false);
        current.stateMachine.SM.SetState(new HumanDefendState(current.stateMachine, opposingTeam));
    }

    private void SwitchControlToDefender(Player defender)
    {
        currentPlayerIndex = team.players.IndexOf(defender);
        defender.SetControlled(true);
        defender.stateMachine.SM.SetState(new HumanDefendState(defender.stateMachine, opposingTeam));
        UpdateUIFollowTarget();
    }
    private void SetPlayerToCorrectHumanState(Player player)
    {
        bool attacking = TeamIsAttacking();
        //Debug.Log($"{team.name} SetPlayerToCorrectHumanState: player={player.name}, attacking={attacking}");
        if (attacking)
        {
            player.stateMachine.SM.SetState(new IdleState(player.stateMachine));
        }
        else
        {
            player.stateMachine.SM.SetState(new HumanDefendState(player.stateMachine, opposingTeam));
        }
    }

    private void ProcessPassInput(Player player)
    {
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        if (KeybindManager.IsKeyDown(binds.passLeft)) player.stateMachine.RequestPass(leftSide: true);
        if (KeybindManager.IsKeyDown(binds.passRight)) player.stateMachine.RequestPass(leftSide: false);
    }

    private void ProcessTackleInput(Player player)
    {
        if (!IsPlayerDefending(player)) return;
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        if (KeybindManager.IsKeyDown(binds.tackle)) player.stateMachine.RequestTackle();
    }
    private bool IsPlayerDefending(Player player)
    {
        return player.stateMachine.SM.CurrentState is HumanDefendState;
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

        if (!initialPlayerSet) return;
        //Debug.Log($"{team.name} TeamInputHandler: newHolder={newHolder?.name}, isOnThisTeam={NewHolderIsOnThisTeam(newHolder)}");
        if (NewHolderIsOnThisTeam(newHolder))
        {
            SwitchControlToNewHolder(newHolder);
        }
        else
        {
            TransitionCurrentPlayerToCorrectState();
        }
    }
    private bool NewHolderIsOnThisTeam(Transform newHolder)
    {
        return newHolder != null && FindPlayerIndex(newHolder) >= 0;
    }
    private void SwitchControlToNewHolder(Transform newHolder)
    {
        int index = FindPlayerIndex(newHolder);
        if (index < 0)
        {
            //Debug.LogWarning($"{team.name} could not find player index for {newHolder?.name}");
            return;
        }
        //Debug.Log($"{team.name} switching control to index {index}, player={newHolder?.name}");
        SetControlledPlayer(index);
    }
    private void TransitionCurrentPlayerToCorrectState()
    {
        Player currentPlayer = GetCurrentPlayer();
        if (!ValidateCurrentPlayer(currentPlayer)) return;
        if (!currentPlayer.isControlled) return;

        SetPlayerToCorrectHumanState(currentPlayer);
    }

    private void UpdatePassIndicatorUI(Player player)
    {
        if (passIndicatorUI == null) return;
        if (!ValidateProbabilityAlgorithm()) return;

        Ball ball = GetBall();

        if (!PlayerIsHoldingBall(player, ball))
        {
            passIndicatorUI.HideAll();
            return;
        }

        UpdateLeftPassIndicator(player, ball);
        UpdateRightPassIndicator(player, ball);
    }
    private void UpdateLeftPassIndicator(Player player, Ball ball)
    {
        Player target = team.FindBestPassTarget(player, leftSide: true);

        if (target != null)
        {
            ShowLeftIndicatorForTarget(player, ball, target);
        }
        else
        {
            passIndicatorUI.HideLeft();
        }
    }
    private void UpdateRightPassIndicator(Player player, Ball ball)
    {
        Player target = team.FindBestPassTarget(player, leftSide: false);

        if (target != null)
        {
            ShowRightIndicatorForTarget(player, ball, target);
        }
        else
        {
            passIndicatorUI.HideRight();
        }
    }
    private void ShowLeftIndicatorForTarget(Player player, Ball ball, Player target)
    {
        var (prob, zone) = CalculatePassResult(ball, player, target);
        passIndicatorUI.ShowLeft(zone, prob);
    }
    private void ShowRightIndicatorForTarget(Player player, Ball ball, Player target)
    {
        var (prob, zone) = CalculatePassResult(ball, player, target);
        passIndicatorUI.ShowRight(zone, prob);
    }
    private (float probability, PassZone zone) CalculatePassResult(Ball ball, Player passer, Player target)
    {
        float dist = CalculatePassDistance(ball, target);
        return probabilityAlgorithm.CalculatePassProbability(dist, passer.handlingStat);
    }
    private float CalculatePassDistance(Ball ball, Player target)
    {
        return Vector3.Distance(ball.transform.position, target.transform.position);
    }

    private bool TeamIsAttacking()
    {
        Ball ball = GetBall();
        if (ball == null || ball.currentHolder == null) return false;
        return TeamOwnsBall(ball);
    }
    private bool TeamOwnsBall(Ball ball)
    {
        foreach (Player p in team.players)
        {
            if (PlayerHoldsBall(p, ball)) return true;
        }
        return false;
    }
    private bool PlayerHoldsBall(Player player, Ball ball)
    {
        return player != null && player.transform == ball.currentHolder;
    }
    private bool PlayerIsHoldingBall(Player player, Ball ball)
    {
        return ball != null && ball.currentHolder == player.transform;
    }
    private int FindPlayerIndex(Transform t)
    {
        for (int i = 0; i < team.players.Count; i++)
        {
            if (PlayerTransformMatchesIndex(t, i)) return i;
        }
        return -1;
    }
    private bool PlayerTransformMatchesIndex(Transform t, int index)
    {
        return team.players[index] != null && team.players[index].transform == t;
    }
    private void UpdateUIFollowTarget()
    {
        if (passIndicatorUI == null) return;
        passIndicatorUI.SetFollowTarget(team.players[currentPlayerIndex].transform);
    }
    private Player GetCurrentPlayer()
    {
        if (!IsValidPlayerIndex(currentPlayerIndex)) return null;
        return team.players[currentPlayerIndex];
    }
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }
    private bool IsValidPlayerIndex(int index)
    {
        return index >= 0 && index < team.players.Count && team.players[index] != null;
    }
    private int ClampPlayerIndex(int index)
    {
        return Mathf.Clamp(index, 0, team.players.Count - 1);
    }
    private bool ValidateTeamHasPlayers()
    {
        return team != null && team.players != null && team.players.Count > 0;
    }
    private bool ValidateCurrentPlayer(Player player)
    {
        return player != null;
    }
    private bool ValidateConfig()
    {
        return config != null;
    }
    private bool ValidateProbabilityAlgorithm()
    {
        return probabilityAlgorithm != null;
    }
    private void DebugMissingConfig()
    {
        Debug.LogWarning("TeamInputHandler: Missing GameConfig reference.");
    }
}

/*private void InitialiseInputActions()
    {
        if (useArrowKeys) return; // Team B uses direct input

        input = GetComponent<PlayerInput>();
        var actions = input.actions.FindActionMap("Player", throwIfNotFound: true);

        moveAction = actions["Move"];
        sprintAction = actions["Sprint"];
        nextAction = actions["Next"];
        prevAction = actions["Previous"];
        passLeftAction = actions["PassLeft"];
        passRightAction = actions["PassRight"];
        tackleAction = actions["Tackle"];
    }*/