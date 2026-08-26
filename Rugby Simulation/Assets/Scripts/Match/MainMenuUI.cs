using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Slider halfLengthSlider;
    [SerializeField] private TextMeshProUGUI halfLengthLabel;

    [Header("Navigation")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        InitialiseSlider();
    }

    private void InitialiseSlider()
    {
        if (halfLengthSlider == null) return;

        halfLengthSlider.minValue = MatchSettingsManager.MinHalfMinutes;
        halfLengthSlider.maxValue = MatchSettingsManager.MaxHalfMinutes;
        halfLengthSlider.wholeNumbers = true;
        if (MatchSettingsManager.Instance != null)
        {
            halfLengthSlider.value = MatchSettingsManager.Instance.HalfLengthMinutes;
        }
        else
        {
            halfLengthSlider.value = MatchSettingsManager.MinHalfMinutes;
        }

        halfLengthSlider.onValueChanged.AddListener(OnSliderChanged);
        UpdateLabel((int)halfLengthSlider.value);
    }
    private void OnSliderChanged(float value)
    {
        int minutes = (int)value;
        MatchSettingsManager.Instance?.SetHalfLength(minutes);
        UpdateLabel(minutes);
    }

    private void UpdateLabel(int minutes)
    {
        if (halfLengthLabel != null)
        {
            halfLengthLabel.text = $"{minutes} min per half";
        }
    }

    public void OnStartPressed()
    {
        MatchSettingsManager.Instance?.ApplySettings();
        SceneManager.LoadScene(gameSceneName);
    }
}
