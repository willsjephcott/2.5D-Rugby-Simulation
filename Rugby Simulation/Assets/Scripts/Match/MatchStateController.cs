using UnityEngine;

public class MatchStateController : MonoBehaviour
{
    public AttackingFormationController teamAAttackController;
    public DefensiveLineController teamADefenceController;

    public AttackingFormationController teamBAttackController;
    public DefensiveLineController teamBDefenceController;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        SubscribeToPossessionEvents();
        SetInitialControllerStates();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPossessionEvents();
    }
    private void SetInitialControllerStates()
    {
        if (MatchManager.Instance == null) return;

        Team startingTeam = MatchManager.Instance.PossessionTeam;
        ApplyControllerStates(startingTeam);
    }
    private void SubscribeToPossessionEvents()
    {
        if (MatchManager.Instance == null) return;
        MatchManager.Instance.OnPossessionChanged += OnPossessionChanged;
    }

    private void UnsubscribeFromPossessionEvents()
    {
        if (MatchManager.Instance == null) return;
        MatchManager.Instance.OnPossessionChanged -= OnPossessionChanged;
    }
    private void OnPossessionChanged(Team possessingTeam)
    {
        //Debug.Log($"MatchStateController: possession changed to {possessingTeam?.name}, isLineoutActive={MatchManager.Instance.IsLineoutActive()}, isRuckActive={MatchManager.Instance.IsRuckActive()}");
        ApplyControllerStates(possessingTeam);
        DebugLogPossessionSwap(possessingTeam);
    }

    private void ApplyControllerStates(Team possessingTeam)
    {
        if (MatchManager.Instance.IsConversionActive()) return;

        if (MatchManager.Instance.IsRuckActive()) return;
        if (MatchManager.Instance.IsLineoutActive())
        {
            DisableAllControllers();
            return;
        }

        Debug.Log($"ApplyControllerStates: teamAAttacking={IsTeamA(possessingTeam)}");
        if (possessingTeam == null)
        {
            // Don't disable controllers if a ruck is in progress
            if (MatchManager.Instance.IsRuckActive()) return;

            DisableAllControllers();
            return;
        }

        bool teamAAttacking = IsTeamA(possessingTeam);
        Debug.Log($"ApplyControllerStates: teamAAttacking={teamAAttacking}, " + $"teamAAttack={teamAAttackController?.enabled}, " + $"teamADefence={teamADefenceController?.enabled}, " + $"teamBAttack={teamBAttackController?.enabled}, " + $"teamBDefence={teamBDefenceController?.enabled}");
        SetTeamAControllers(attacking: teamAAttacking);
        SetTeamBControllers(attacking: !teamAAttacking);

    }
    private void SetTeamAControllers(bool attacking)
    {
        SetControllerActive(teamAAttackController, attacking);
        SetControllerActive(teamADefenceController, !attacking);
    }

    private void SetTeamBControllers(bool attacking)
    {
        SetControllerActive(teamBAttackController, attacking);
        SetControllerActive(teamBDefenceController, !attacking);
    }

    private void SetControllerActive(MonoBehaviour controller, bool active)
    {
        if (controller == null) return;
        controller.enabled = active;
    }

    private void DisableAllControllers()
    {
        SetControllerActive(teamAAttackController, false);
        SetControllerActive(teamADefenceController, false);
        SetControllerActive(teamBAttackController, false);
        SetControllerActive(teamBDefenceController, false);
    }
    private bool IsTeamA(Team team)
    {
        if (MatchManager.Instance == null) return false;
        return team == MatchManager.Instance.TeamA;
    }
    private void ValidateReferences()
    {
        if (teamAAttackController == null) DebugLogMissing("teamAAttackController");
        if (teamADefenceController == null) DebugLogMissing("teamADefenceController");
        if (teamBAttackController == null) DebugLogMissing("teamBAttackController");
        if (teamBDefenceController == null) DebugLogMissing("teamBDefenceController");
    }
    private void DebugLogPossessionSwap(Team team)
    {
        string teamName;

        if (team != null)
        {
            teamName = team.name;
        }
        else
        {
            teamName = "none";
        }
        Debug.Log($"Possession changed to: {teamName}. Controllers swapped.");
    }

    private void DebugLogMissing(string fieldName)
    {
        Debug.LogWarning($"MatchStateController: {fieldName} is not assigned.");
    }

}
