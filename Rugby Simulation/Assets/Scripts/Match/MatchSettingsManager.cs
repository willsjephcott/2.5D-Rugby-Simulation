using UnityEngine;

public class MatchSettingsManager : MonoBehaviour
{
    public static MatchSettingsManager Instance;

    public const int MinHalfMinutes = 3;
    public const int MaxHalfMinutes = 10;

    public int HalfLengthMinutes { get; private set; } = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHalfLength(int minutes)
    {
        HalfLengthMinutes = Mathf.Clamp(minutes, MinHalfMinutes, MaxHalfMinutes);
    }
    public void ApplySettings()
    {
        MatchTimerManager.HalfLengthSeconds = HalfLengthMinutes * 60;
        MatchTimerManager.ClockSpeed = 40f / HalfLengthMinutes;
    }
}
