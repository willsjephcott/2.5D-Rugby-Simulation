using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversionResultUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;
    public Button continueButton;

    private void Awake()
    {
        Hide();
        SetupButton();
    }

    public void Show(bool success)
    {
        if (panel != null) panel.SetActive(true);

        UpdateResultText(success);
        UpdateScoreText();
        PauseGame();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
    private void SetupButton()
    {
        if (continueButton == null) return;
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinuePressed);
    }

    private void UpdateResultText(bool success)
    {
        if (resultText == null) return;
        if (success)
        {
            resultText.text = "Conversion Made!";
        }
        else
        {
            resultText.text = "Missed!";
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        if (ScoreManager.Instance == null) return;

        string teamAName = GetTeamName(MatchManager.Instance?.TeamA, "Team A");
        string teamBName = GetTeamName(MatchManager.Instance?.TeamB, "Team B");

        scoreText.text = $"{teamAName}: {ScoreManager.Instance.TeamAScore}  -  {teamBName}: {ScoreManager.Instance.TeamBScore}";
    }

    private string GetTeamName(Team team, string fallback)
    {
        if (team != null)
        {
            return team.name;
        }

        return fallback;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    private void OnContinuePressed()
    {
        Hide();
        ResumeGame();
        ScoreManager.Instance.FinaliseScore(null);
    }

    //Not used anymore
    /*private void TriggerKickoff()
    {
        KickOffResetter resetter = FindAnyObjectByType<KickOffResetter>();
        if (resetter != null)
        {
            resetter.ResetToKickoff();
            return;
        }
        DebugLogMissingResetter();
    }

    private void DebugLogMissingResetter()
    {
        Debug.LogWarning("ConversionResultUI: KickOffResetter not found in scene.");
    }*/
}