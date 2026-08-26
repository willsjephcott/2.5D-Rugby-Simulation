using UnityEngine;


public class ControlHUDManager : MonoBehaviour
{


    [SerializeField] private ControlHUDPanel teamAPanel;
    [SerializeField] private ControlHUDPanel teamBPanel;
    [SerializeField] private Team teamA;
    [SerializeField] private Team teamB;


    private void Start()
    {
        SubscribeToPossession();
        RefreshBothPanels();   // set initial state
    }

    private void OnDestroy()
    {
        UnsubscribeFromPossession();
    }


    private void Update()
    {
        RefreshBothPanels();
    }


    private void SubscribeToPossession()
    {
        if (MatchManager.Instance == null) return;
        MatchManager.Instance.OnPossessionChanged += OnPossessionChanged;
    }

    private void UnsubscribeFromPossession()
    {
        if (MatchManager.Instance == null) return;
        MatchManager.Instance.OnPossessionChanged -= OnPossessionChanged;
    }

    private void OnPossessionChanged(Team newPossessor)
    {
        RefreshBothPanels();
    }

    private void RefreshBothPanels()
    {
        if (!IsMatchReady())
        {
            HideBothPanels();
            return;
        }

        if (IsSetPieceActive())
        {
            HideBothPanels();
            return;
        }

        Team possessor = MatchManager.Instance.PossessionTeam;

        RefreshPanel(teamAPanel, isTeamA: true, possessor);
        RefreshPanel(teamBPanel, isTeamA: false, possessor);
    }

    private void RefreshPanel(ControlHUDPanel panel, bool isTeamA, Team possessor)
    {
        if (panel == null) return;

        // If nobody has the ball yet, hide.
        if (possessor == null)
        {
            panel.Hide();
            return;
        }

        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        bool isAttacking = IsThisTeamAttacking(isTeamA, possessor);

        if (isAttacking)
        {
            panel.ShowAttack(binds.passLeft, binds.passRight);
        }
        else
        {
            panel.ShowDefend(binds.tackle, binds.switchDefender);
        }
    }


    private bool IsMatchReady()
    {
        return MatchManager.Instance != null && teamA != null && teamB != null;
    }

    private bool IsSetPieceActive()
    {
        return MatchManager.Instance.IsRuckActive() || MatchManager.Instance.IsLineoutActive() || MatchManager.Instance.IsConversionActive();
    }

    private bool IsThisTeamAttacking(bool isTeamA, Team possessor)
    {
        Team thisTeam;

        if (isTeamA) thisTeam = teamA;
        else
        {
            thisTeam = teamB; 
        }
        return possessor == thisTeam;
    }

    private void HideBothPanels()
    {
        teamAPanel?.Hide();
        teamBPanel?.Hide();
    }
}