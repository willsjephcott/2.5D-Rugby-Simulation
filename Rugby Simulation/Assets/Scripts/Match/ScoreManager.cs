using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TeamAScore { get; private set; }
    public int TeamBScore { get; private set; }

    public const int PointsPerTry = 5;
    public const int PointsPerConversion = 2;

    // Fired first — ConversionController listens here to run the QTE
    public event Action<Team, Vector3> OnTryScoredPreConversion;

    // Fired after conversion resolves — ScoreScreenUI listens here (unchanged signature)
    public event Action<Team, int, int> OnTryScored;

    public event Action<Vector3> OnBallOutOfBounds;

    bool tryFiredThisPhase;
    Team pendingScoringTeam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by TryLineTrigger
    public void RegisterTry(Team scoringTeam)
    {
        if (tryFiredThisPhase) return;
        if (!ValidateScoringTeam(scoringTeam)) return;

        tryFiredThisPhase = true;
        pendingScoringTeam = scoringTeam;

        AwardPoints(scoringTeam, PointsPerTry);
        DebugLogTry(scoringTeam);

        Vector3 tryPosition = GetBallPosition();

        Debug.Log("ScoreManager: About to find ConversionController");
        ConversionController conversion = UnityEngine.Object.FindAnyObjectByType<ConversionController>();
        Debug.Log($"ScoreManager: ConversionController found={conversion != null}");
        if (conversion != null)
            conversion.HandleTryScored(scoringTeam, tryPosition);
        else
            FinaliseScore(scoringTeam);
    }

    // Called by ConversionController after QTE resolves (success only)
    public void RegisterConversion()
    {
        if (!ValidatePendingTeam()) return;
        AwardPoints(pendingScoringTeam, PointsPerConversion);
        DebugLogConversion();
    }

    // Called by ConversionController to show the score screen
    public void FinaliseScore(Team scoringTeam)
    {
        Team team;

        if (scoringTeam != null)
        {
            team = scoringTeam;
        }
        else
        {
            team = pendingScoringTeam;
        }
        OnTryScored?.Invoke(team, TeamAScore, TeamBScore);
    }

    // Called by TouchlineTrigger
    public void RegisterOutOfBounds(Vector3 position)
    {
        DebugLogOutOfBounds();
        OnBallOutOfBounds?.Invoke(position);
    }

    // Called by KickoffResetter after reset so tries can fire again
    public void ResetPhase()
    {
        tryFiredThisPhase = false;
        pendingScoringTeam = null;
    }

    public void ResetScores()
    {
        TeamAScore = 0;
        TeamBScore = 0;
    }

    private void AwardPoints(Team scoringTeam, int points)
    {
        MatchManager manager = MatchManager.Instance;
        if (manager == null) return;

        if (scoringTeam == manager.TeamA) TeamAScore += points;
        else if (scoringTeam == manager.TeamB) TeamBScore += points;
    }

    private Vector3 GetBallPosition()
    {
        Ball ball = MatchManager.Instance?.ball;
        if (ball != null)
        {
            return ball.transform.position;
        }

        return Vector3.zero;
    }

    private bool HasConversionListeners()
    {
        return OnTryScoredPreConversion != null;
    }

    private bool ValidateScoringTeam(Team scoringTeam)
    {
        if (scoringTeam == null)
        {
            DebugLogMissingScoringTeam();
            return false;
        }
        if (MatchManager.Instance == null)
        {
            DebugLogMissingMatchManager();
            return false;
        }
        return true;
    }

    private bool ValidatePendingTeam()
    {
        if (pendingScoringTeam != null) return true;
        DebugLogMissingPendingTeam();
        return false;
    }

    private void DebugLogTry(Team scoringTeam)
    {
        Debug.Log($"TRY! {scoringTeam.name} scores. TeamA {TeamAScore} - TeamB {TeamBScore}");
    }

    private void DebugLogConversion()
    {
        Debug.Log($"Conversion scored. TeamA {TeamAScore} - TeamB {TeamBScore}");
    }

    private void DebugLogOutOfBounds()
    {
        Debug.Log("ScoreManager: RegisterOutOfBounds fired");
    }

    private void DebugLogMissingScoringTeam()
    {
        Debug.LogWarning("ScoreManager: scoringTeam is null.");
    }

    private void DebugLogMissingMatchManager()
    {
        Debug.LogWarning("ScoreManager: MatchManager not found.");
    }

    private void DebugLogMissingPendingTeam()
    {
        Debug.LogWarning("ScoreManager: RegisterConversion called but pendingScoringTeam is null.");
    }
}
/*using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TeamAScore { get; private set; }
    public int TeamBScore { get; private set; }

    public const int PointsPerTry = 5;

    public event Action<Team, Vector3> OnTryScoredPreConversion;
    public event Action<Team, int, int> OnTryScored;      // (scoringTeam, teamAScore, teamBScore)
    public event Action<Vector3> OnBallOutOfBounds;        // for lineout logic later

    bool tryFiredThisPhase = false;
    Team pendingScoringTeam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by TryLineTrigger
    public void RegisterTry(Team scoringTeam)
    {
        if (tryFiredThisPhase) return;
        if (scoringTeam == null) return;

        tryFiredThisPhase = true;
        pendingScoringTeam = scoringTeam;

        MatchManager manager = MatchManager.Instance;
        if (manager == null) return;

        if (scoringTeam == manager.TeamA) TeamAScore += PointsPerTry;
        else if (scoringTeam == manager.TeamB) TeamBScore += PointsPerTry;

        Debug.Log($"TRY! {scoringTeam.name} scores. TeamA {TeamAScore} - TeamB {TeamBScore}");
        OnTryScored?.Invoke(scoringTeam, TeamAScore, TeamBScore);
    }
    // Called by TouchlineTrigger — fires event for future lineout logic
    public void RegisterOutOfBounds(Vector3 position)
    {
        Debug.Log("ScoreManager: RegisterOutOfBounds fired");
        OnBallOutOfBounds?.Invoke(position);
    }

    // Called by KickoffResetter after reset so tries can fire again
    public void ResetPhase()
    {
        tryFiredThisPhase = false;
    }

    public void ResetScores()
    {
        TeamAScore = 0;
        TeamBScore = 0;
    }
}
*/