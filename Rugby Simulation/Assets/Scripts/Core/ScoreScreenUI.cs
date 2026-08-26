using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scorePanel;           // The whole overlay panel
    public TextMeshProUGUI teamAScoreText;  // e.g. "Team A: 5"
    public TextMeshProUGUI teamBScoreText;  // e.g. "Team B: 0"
    public TextMeshProUGUI tryBannerText;   // e.g. "TRY! Team A scores!"
    public Button kickoffButton;            // "Kick Off" button

    public KickOffResetter kickoffResetter;

    Team lastScoringTeam;

    private void Start()
    {
        HideScoreScreen();
        SubscribeToEvents();
        SetupButton();
        RefreshScoreDisplay();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    private void RefreshScoreDisplay()
    {
        if (ScoreManager.Instance == null) return;
        UpdateScoreTexts(null, ScoreManager.Instance.TeamAScore, ScoreManager.Instance.TeamBScore);
    }

    private void SubscribeToEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnTryScored += HandleTryScored;
        }
        else
        {
            Debug.LogWarning("ScoreScreenUI: ScoreManager not found.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnTryScored -= HandleTryScored;
        }
    }
    private void SetupButton()
    {
        if (kickoffButton == null) return;
        kickoffButton.onClick.RemoveAllListeners();
        kickoffButton.onClick.AddListener(OnKickoffPressed);
    }

    private void HandleTryScored(Team scoringTeam, int teamAScore, int teamBScore)
    {
        lastScoringTeam = scoringTeam;
        UpdateScoreTexts(scoringTeam, teamAScore, teamBScore);
        ShowScoreScreen();
        PauseGame();
    }

    private void UpdateScoreTexts(Team scoringTeam, int teamAScore, int teamBScore)
    {
        MatchManager manager = MatchManager.Instance;

        if (teamAScoreText != null)
        {
            string teamAName = "Team A";

            if (manager != null && manager.TeamA != null && manager.TeamA.name != null)
            {
                teamAName = manager.TeamA.name;
            }
            teamAScoreText.text = $"{teamAName}: {teamAScore}";
        }

        if (teamBScoreText != null)
        {
            string teamBName = "Team B";

            if (manager != null && manager.TeamB != null && manager.TeamB.name != null)
            {
                teamBName = manager.TeamB.name;
            }
            teamBScoreText.text = $"{teamBName}: {teamBScore}";
        }

        if (tryBannerText != null)
        {
            string teamName = "Team";

            if (scoringTeam != null && scoringTeam.name != null)
            {
                teamName = scoringTeam.name;
            }

            tryBannerText.text = $"TRY! {teamName} scores!";
        }
    }

    private void ShowScoreScreen()
    {
        if (scorePanel != null) scorePanel.SetActive(true);
    }

    private void HideScoreScreen()
    {
        if (scorePanel != null) scorePanel.SetActive(false);
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void OnKickoffPressed()
    {
        Debug.Log("1. Hiding score screen");
        HideScoreScreen();

        Debug.Log($"2. About to reset - Time.timeScale={Time.timeScale}");
        kickoffResetter?.ResetToKickoff(lastScoringTeam);

        Debug.Log($"3. About to resume - Time.timeScale={Time.timeScale}");
        ResumeGame();

        Debug.Log($"4. Done - Time.timeScale={Time.timeScale}");

        if (kickoffResetter != null)
        {
            kickoffResetter.ResetToKickoff();
        }
        else
        {
            Debug.LogWarning("ScoreScreenUI: KickoffResetter not assigned.");
        }
    }

}
