using System;
using UnityEngine;

public class MatchTimerManager : MonoBehaviour
{
    public static MatchTimerManager Instance { get; private set; }
    public static float ClockSpeed {get; set;} = 10f;

    public event Action OnHalfTime;
    public event Action OnFullTime;
    public event Action<int, int> OnTimerTick;

    float realTimeElapsed = 0f;

    public static int HalfLengthSeconds { get; set; } = 180;

    public int CurrentHalf { get; private set; } = 1;
    public bool MatchOver { get; private set; } = false;
    public bool IsInExtraTime { get; private set; } = false;
    public float ElapsedThisHalf { get; private set; } = 0;

    private bool running = false;
    private float tickAccumulator = 0;
    private int lastTickedSecond = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update()
    {
        if (!running || MatchOver) return;
        AdvanceClock();
        TryFireTick();
        CheckForTimeUp();
    }

    private void AdvanceClock()
    {
        realTimeElapsed += Time.deltaTime;
        ElapsedThisHalf += Time.deltaTime * ClockSpeed;
        tickAccumulator += Time.deltaTime * ClockSpeed;
    }
    private void TryFireTick()
    {
        if (tickAccumulator < 1f) return;
        tickAccumulator -= 1f;

        int totalSecs = Mathf.FloorToInt(ElapsedThisHalf);
        if (totalSecs == lastTickedSecond) return;

        lastTickedSecond = totalSecs;
        OnTimerTick?.Invoke(totalSecs / 60, totalSecs % 60); // first is minutes the rest is seconds
    }
    private void CheckForTimeUp()
    {
        if (!IsInExtraTime && realTimeElapsed >= HalfLengthSeconds) IsInExtraTime = true;
    }
    public void NotifyDeadBall()
    {
        if (!IsInExtraTime || MatchOver) return;
        if (CurrentHalf == 1) TransitionToSecondHalf();
        else TriggerFullTime();
    }

    private void TransitionToSecondHalf()
    {
        CurrentHalf = 2;
        ResetHalfState();
        running = false;
        OnHalfTime?.Invoke();
    }
    private void TriggerFullTime()
    {
        MatchOver = true;
        running = false;
        OnFullTime?.Invoke();
    }

    private void ResetHalfState()
    {
        ElapsedThisHalf = 0f;
        realTimeElapsed = 0f;
        IsInExtraTime = false;
        tickAccumulator = 0f;
        lastTickedSecond = -1;
    }
    public void StartTimer()
    {
        running = true;
    }
    public void StopTimer() 
    { 
        running = false; 
    }
    public void ResetTimer()
    {
        running = false;
        MatchOver = false;
        CurrentHalf = 1;
        ResetHalfState();
    }
    public string GetDisplayTime()
    {
        int total = Mathf.FloorToInt(ElapsedThisHalf);
        return $"{total / 60:D2}:{total % 60:D2}"; //Decimal, 2 sf for D2
    }

}
