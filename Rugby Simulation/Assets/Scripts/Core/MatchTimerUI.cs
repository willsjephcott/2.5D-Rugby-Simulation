using TMPro;
using UnityEngine;

public class MatchTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerLabel;
    [SerializeField] private TextMeshProUGUI halfLabel;
    [SerializeField] private TextMeshProUGUI extraTimeIndicator;
    [SerializeField] private GameObject halfTimePanel;
    [SerializeField] private GameObject fullTimePanel;
    [SerializeField] private KickoffManager kickoffManager;

    private void OnEnable()
    {
        MatchTimerManager.Instance.OnTimerTick += HandleTick;
        MatchTimerManager.Instance.OnHalfTime += HandleHalfTime;
        MatchTimerManager.Instance.OnFullTime += HandleFullTime;
    }

    private void OnDisable()
    {
        MatchTimerManager.Instance.OnTimerTick -= HandleTick;
        MatchTimerManager.Instance.OnHalfTime -= HandleHalfTime;
        MatchTimerManager.Instance.OnFullTime -= HandleFullTime;
    }

    private void Start()
    {
        HideAllOverlays();
        SetHalfLabel(1);
        SetTimerLabel(0, 0);
    }
    public void OnStartSecondHalfPressed()
    {
        HideHalfTimePanel();
        kickoffManager.StartSecondHalf();
    }
    private void HandleTick(int minutes, int seconds)
    {
        SetTimerLabel(minutes, seconds);
        SetExtraTimeVisible(MatchTimerManager.Instance.IsInExtraTime);
    }

    private void HandleHalfTime()
    {
        SetHalfLabel(2);
        SetTimerLabel(0, 0);
        SetExtraTimeVisible(false);
        ShowPanel(halfTimePanel);
    }

    private void HandleFullTime()
    {
        ShowPanel(fullTimePanel);
    }

    private void SetTimerLabel(int minutes, int seconds)
    {
        if (timerLabel) timerLabel.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void SetHalfLabel(int half)
    {
        if (halfLabel) halfLabel.text = $"HALF {half}";
    }

    private void SetExtraTimeVisible(bool visible)
    {
        if (extraTimeIndicator) extraTimeIndicator.gameObject.SetActive(visible);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel) panel.SetActive(true);
    }
    private void HideAllOverlays()
    {
        if (halfTimePanel) halfTimePanel.SetActive(false);
        if (fullTimePanel) fullTimePanel.SetActive(false);
        SetExtraTimeVisible(false);
    }

    public void HideHalfTimePanel()
    {
        if (halfTimePanel) halfTimePanel.SetActive(false);
    }
}
