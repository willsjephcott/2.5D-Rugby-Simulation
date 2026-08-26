using UnityEngine;
using System.Collections.Generic;

public class KickoffManager : MonoBehaviour
{
    [SerializeField] private MatchTimerUI matchTimerUI;
    [SerializeField] private LineoutController lineoutController;

    public Team attackingTeam;
    public Team defendingTeam;
    public float defensiveLineOffset = 10f;
    public float playerSpacing = 2f;
    public float groundY = 0f;
    public Vector3 kickoffCentre = Vector3.zero;

    private void Start()
    {
        MatchSettingsManager.Instance?.ApplySettings();
        Debug.Log($"Timer after ApplySettings: HalfLengthSeconds={MatchTimerManager.HalfLengthSeconds}, ClockSpeed={MatchTimerManager.ClockSpeed}");
        Debug.Log($"MatchSettingsManager.HalfLengthMinutes={MatchSettingsManager.Instance?.HalfLengthMinutes}");
        PositionTeamsForKickoff();
        GiveBallToFirstAttacker();
        MatchTimerManager.Instance?.StartTimer();
        MatchTimerManager.Instance.OnHalfTime += StartSecondHalf;
    }
    private void OnDestroy()
    {
        if (MatchTimerManager.Instance != null)
            MatchTimerManager.Instance.OnHalfTime -= StartSecondHalf;
    }
    public void PositionTeamsForKickoff()
    {
        if (!ValidateTeams()) return;

        PositionAttackingTeam();
        PositionDefendingTeam();
    }
    public void StartSecondHalf()
    {
        Debug.Log($"StartSecondHalf: lineoutController={lineoutController}");
        lineoutController?.ForceReset();
        MatchManager.Instance?.ClearActiveLineout();
        PositionTeamsForKickoff();
        GiveBallToFirstAttacker();
        MatchTimerManager.Instance?.StartTimer();
        //matchTimerUI?.HideHalfTimePanel();
        
    }
    private void PositionAttackingTeam()
    {
        PositionTeamAtCentre(attackingTeam, kickoffCentre);
    }
    private void PositionDefendingTeam()
    {
        Vector3 defensiveCentre = CalculateDefensiveCentre();
        PositionTeamAtCentre(defendingTeam, defensiveCentre);
    }
    private Vector3 CalculateDefensiveCentre()
    {
        Vector3 attackDir = GetNormalisedAttackDirection();
        return kickoffCentre + attackDir * defensiveLineOffset;
    }
    private void PositionTeamAtCentre(Team team, Vector3 centre)
    {
        List<Player> players = team.players;
        if (!HasPlayers(players)) return;

        Vector3 lateral = CalculateLateralDirection();

        for (int i = 0; i < players.Count; i++)
        {
            PositionPlayer(players[i], centre, lateral, i, players.Count);
        }
    }
    private void PositionPlayer(Player player, Vector3 centre, Vector3 lateral, int index, int totalCount)
    {
        if (player == null) return;

        Vector3 position = CalculatePlayerPosition(centre, lateral, index, totalCount);
        ApplyPlayerPosition(player, position);
    }
    private Vector3 CalculatePlayerPosition(Vector3 centre, Vector3 lateral, int index, int totalCount)
    {
        float offset = CalculateLateralOffset(index, totalCount);
        Vector3 position = centre + lateral * offset;
        position.y = groundY;
        return position;
    }
    private float CalculateLateralOffset(int index, int totalCount)
    {
        float centreOffset = (totalCount - 1) * playerSpacing * 0.5f;
        return (index * playerSpacing) - centreOffset;
    }
    private void ApplyPlayerPosition(Player player, Vector3 position)
    {
        player.transform.position = position;
        ZeroPlayerVelocity(player);
    }
    private void ZeroPlayerVelocity(Player player)
    {
        if (player.rb == null) return;

        player.rb.linearVelocity = Vector3.zero;
        player.rb.angularVelocity = Vector3.zero;
    }
    public void GiveBallToFirstAttacker()
    {
        if (!ValidateBallExists()) return;
        if (!HasPlayers(attackingTeam?.players)) return;

        Player carrier = FindStartingBallCarrier();
        AttachBallToCarrier(carrier);
    }
    private Player FindStartingBallCarrier()
    {
        int midIndex = attackingTeam.players.Count / 2;
        return attackingTeam.players[midIndex];
    }
    private void AttachBallToCarrier(Player carrier)
    {
        if (carrier == null) return;

        Ball ball = GetBall();
        ball.AttachTo(carrier.transform);
        DebugLogBallGiven(carrier);
    }
    private Vector3 GetNormalisedAttackDirection()
    {
        if (attackingTeam == null) return Vector3.forward;

        Vector3 dir = attackingTeam.attackDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        return dir.normalized;
    }
    private Vector3 CalculateLateralDirection()
    {
        Vector3 attackDir = GetNormalisedAttackDirection();
        return Vector3.Cross(Vector3.up, attackDir).normalized;
    }
    private Ball GetBall()
    {
        return MatchManager.Instance?.ball;
    }
    private bool HasPlayers(List<Player> players)
    {
        return players != null && players.Count > 0;
    }
    public void SetReceivingTeam(Team scoringTeam)
    {
        MatchManager manager = MatchManager.Instance;
        if (manager == null) return;

        if (scoringTeam == manager.TeamA)
        {
            attackingTeam = manager.TeamB;
            defendingTeam = manager.TeamA;
        }
        else
        {
            attackingTeam = manager.TeamA;
            defendingTeam = manager.TeamB;
        }
    }
    private void OnDrawGizmos()
    {
        int count = 6;

        if (attackingTeam == null) return;

        Vector3 lateral = CalculateLateralDirection();

        if (attackingTeam != null && attackingTeam.players != null)
        {
            count = attackingTeam.players.Count;
        }

        DrawTeamGizmo(Color.green, kickoffCentre, lateral, count);

        Vector3 defensiveCentre = CalculateDefensiveCentre();

        count = 6;

        if (defendingTeam != null && defendingTeam.players != null)
        {
            count = defendingTeam.players.Count;
        }

        DrawTeamGizmo(Color.red, defensiveCentre, lateral, count);

        DrawOffsetLine(defensiveCentre);
    }
    private void DrawTeamGizmo(Color colour, Vector3 centre, Vector3 lateral, int count)
    {
        Gizmos.color = colour;
        for (int i = 0; i < count; i++)
        {
            float offset = CalculateLateralOffset(i, count);
            Vector3 pos = centre + lateral * offset;
            pos.y = groundY;
            Gizmos.DrawWireSphere(pos, 0.3f);
        }
    }
    private void DrawOffsetLine(Vector3 defensiveCentre)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(kickoffCentre, defensiveCentre);
        Gizmos.DrawWireSphere(defensiveCentre, 0.4f);
    }
    private bool ValidateTeams()
    {
        if (attackingTeam == null)
        {
            DebugLogMissingTeam("attackingTeam");
            return false;
        }
        if (defendingTeam == null)
        {
            DebugLogMissingTeam("defendingTeam");
            return false;
        }
        return true;
    }
    private bool ValidateBallExists()
    {
        if (GetBall() == null)
        {
            DebugLogMissingBall();
            return false;
        }
        return true;
    }
    private void DebugLogBallGiven(Player carrier)
    {
        Debug.Log($"KickoffManager: Ball given to {carrier.name}.");
    }
    private void DebugLogMissingTeam(string fieldName)
    {
        Debug.LogWarning($"KickoffManager: {fieldName} is not assigned.");
    }
    private void DebugLogMissingBall()
    {
        Debug.LogWarning("KickoffManager: Ball not found via MatchManager.");
    }
    
}