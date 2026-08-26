using UnityEngine;
using System;

public class ConversionController : MonoBehaviour
{
    public NeedleController needleController;
    public Transform teamAPostsTarget;
    public Transform teamBPostsTarget;
    public ConversionResultUI conversionResultUI;

    bool waitingForKickFinish;
    float kickTimer;
    bool kickSuccessPending;

    ConversionManager activeConversion;

    private void Update()
    {
        if (!waitingForKickFinish) return;

        kickTimer -= Time.deltaTime;
        if (kickTimer > 0f) return;

        waitingForKickFinish = false;

        MatchManager.Instance.SetConversionActive(false);

        if (activeConversion != null)
        {
            activeConversion.OnConversionComplete -= OnConversionComplete;
            activeConversion = null;
        }

        ShowResultUI(kickSuccessPending);
        ScoreManager.Instance.FinaliseScore(null);
    }
    private Transform SelectPostsTarget(Team scoringTeam)
    {
        if (scoringTeam == MatchManager.Instance?.TeamA) return teamAPostsTarget;
        return teamBPostsTarget;
    }

    public void HandleTryScored(Team scoringTeam, Vector3 tryPosition)
    {
        if (activeConversion != null) return;
        if (!ValidateReferences()) return;

        ConversionConfig config = GetConfig();
        if (!ValidateConfig(config)) return;

        PreparePlayersForConversion();

        activeConversion = BuildConversionManager(scoringTeam, config, tryPosition);
        activeConversion.OnConversionComplete += OnConversionComplete;
        activeConversion.StartConversion();
        


        DebugLogConversionStarted(scoringTeam);
    }

    private void OnConversionComplete(bool success)
    {
        if (success) ScoreManager.Instance.RegisterConversion();

        kickSuccessPending = success;
        waitingForKickFinish = true;
        kickTimer = 1.0f; // finetune
        if (MatchTimerManager.Instance?.IsInExtraTime == true) MatchTimerManager.Instance?.NotifyDeadBall();
    }

    private ConversionManager BuildConversionManager(Team scoringTeam, ConversionConfig config, Vector3 tryPosition)
    {
        return new ConversionManager(scoringTeam, MatchManager.Instance.ball, config, needleController, SelectPostsTarget(scoringTeam), tryPosition);
    }

    private void PreparePlayersForConversion()
    {
        MatchManager.Instance.SetConversionActive(true);
        ReleaseTeam(MatchManager.Instance.TeamA);
        ReleaseTeam(MatchManager.Instance.TeamB);
    }

    private void ReleaseTeam(Team team)
    {
        if (team?.players == null) return;

        foreach (Player player in team.players)
        {
            if (player == null) continue;
            player.SetControlled(false);
            player.stateMachine.SM.SetState(new AIIdleState(player.stateMachine));
        }
    }

    private void ShowResultUI(bool success)
    {
        if (conversionResultUI == null)
        {
            DebugLogMissingResultUI();
            return;
        }
        conversionResultUI.Show(success);
    }

    private ConversionConfig GetConfig()
    {
        return MatchManager.Instance?.config?.conversionConfig;
    }

    private bool ValidateReferences()
    {
        if (needleController != null && teamAPostsTarget != null && teamBPostsTarget != null && MatchManager.Instance?.ball != null) return true;
        DebugLogMissingReferences();
        return false;
    }

    private bool ValidateConfig(ConversionConfig config)
    {
        if (config != null) return true;
        DebugLogMissingConfig();
        return false;
    }

    private void DebugLogConversionStarted(Team scoringTeam)
    {
        Debug.Log($"ConversionController: Conversion started for {scoringTeam.name}");
    }

    private void DebugLogMissingResultUI()
    {
        Debug.LogWarning("ConversionController: ConversionResultUI not assigned.");
    }

    private void DebugLogMissingReferences()
    {
        Debug.LogWarning("ConversionController: Missing NeedleController, postsCentreTarget, or Ball.");
    }

    private void DebugLogMissingConfig()
    {
        Debug.LogWarning("ConversionController: ConversionConfig missing from GameConfig.");
    }
    private void DebugLogSubscribed()
    {
        Debug.Log("ConversionController: Subscribed to OnTryScoredPreConversion");
    }

    private void DebugLogMissingScoreManager()
    {
        Debug.LogWarning("ConversionController: ScoreManager.Instance is null on subscribe.");
    }
}