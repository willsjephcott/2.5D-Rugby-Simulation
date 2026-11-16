using UnityEngine;

public class KickOffResetter : MonoBehaviour
{
    public KickoffManager kickoffManager;

    public void ResetToKickoff(Team scoringTeam = null) // Means if scoringTeam is null it still works or scoringTeam can also not be null
    {
        if (kickoffManager == null)
        {
            Debug.LogWarning("KickoffResetter: KickoffManager not assigned.");
            return;
        }

        if (scoringTeam!= null)
        {
            kickoffManager.SetReceivingTeam(scoringTeam);
        }

        ResetAllPlayerStates();
        kickoffManager.PositionTeamsForKickoff();
        kickoffManager.GiveBallToFirstAttacker();
        RestoreHumanControl();
        ScoreManager.Instance?.ResetPhase();
        FindAnyObjectByType<ConversionResultUI>()?.Hide();
    }


    // ResetTeam hands every player to the AI; without this the team that just scored is left with no controlled player
    private void RestoreHumanControl()
    {
        TeamInputHandler[] handlers = FindObjectsByType<TeamInputHandler>(FindObjectsSortMode.None);
        foreach (TeamInputHandler handler in handlers)
        {
            handler.ReclaimControl();
        }
    }

    private void ResetAllPlayerStates()
    {
        MatchManager manager = MatchManager.Instance;
        if (manager == null) return;

        ResetTeam(manager.TeamA);
        ResetTeam(manager.TeamB);
    }

    private void ResetTeam(Team team)
    {
        if (team?.players == null) return;

        foreach (Player player in team.players)
        {
            if (player == null) continue;
            player.SetControlled(false);
            player.stateMachine.SM.SetState(new AIIdleState(player.stateMachine));
            ZeroVelocity(player);
        }
    }

    private void ZeroVelocity(Player player)
    {
        if (player.rb == null) return;
        player.rb.linearVelocity = Vector3.zero;
        player.rb.angularVelocity = Vector3.zero;
    }
}
