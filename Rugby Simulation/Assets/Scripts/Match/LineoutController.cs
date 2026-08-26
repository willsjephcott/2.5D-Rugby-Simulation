using UnityEngine;

public class LineoutController : MonoBehaviour
{
    public static LineoutController Instance { get; private set; }

    public LineoutSlotSelectorUI slotSelectorUI;
    public NeedleController needleController;

    LineoutManager activeLineout;
    Team lastTouchingTeam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void RegisterOutOfBounds(Vector3 position, Team lastTouchingTeam)
    {
        if (activeLineout != null) return;

        this.lastTouchingTeam = lastTouchingTeam;
        StartLineout(position);
    }
    private void StartLineout(Vector3 position)
    {
        MatchManager manager = MatchManager.Instance;
        if (manager == null) { Debug.LogWarning("LineoutController: ball is null on MatchManager."); return; }

        Team throwingTeam = DetermineThrowingTeam(manager);
        Team defendingTeam = throwingTeam == manager.TeamA ? manager.TeamB : manager.TeamA;

        LineoutConfig config = GetConfig();
        if (config == null)
        {
            Debug.LogWarning("LineoutController: LineoutConfig missing from GameConfig.");
            return;
        }

        Vector3 lineoutPosition = CalculateLineoutPosition(position, config);

        //Debug.Log($"LineoutManager created. Throwing: {throwingTeam.name}, Defending: {defendingTeam.name}");
        activeLineout = new LineoutManager(throwingTeam, defendingTeam, manager.ball, config, lineoutPosition);
        activeLineout.OnFormingComplete += OnFormingComplete;
        activeLineout.OnLineoutComplete += OnLineoutComplete;
        manager.RegisterLineout(activeLineout);
        activeLineout.StartLineout();

    }
    public void ForceReset()
    {
        Debug.Log("LineoutController: ForceReset called");
        if (activeLineout != null)
        {
            activeLineout.OnFormingComplete -= OnFormingComplete;
            activeLineout.OnLineoutComplete -= OnLineoutComplete;
            activeLineout = null;
        }
        slotSelectorUI?.Hide();
        MatchManager.Instance?.ClearActiveLineout();
    }
    private void OnFormingComplete()
    {
        if (slotSelectorUI == null)
        {
            Debug.LogWarning("LineoutController: SlotSelectorUI not assigned.");
            return;
        }

        slotSelectorUI.Show(activeLineout);
    }
    private void OnLineoutComplete()
    {
        activeLineout.OnFormingComplete -= OnFormingComplete;
        activeLineout.OnLineoutComplete -= OnLineoutComplete;
        activeLineout = null;

        MatchManager.Instance?.ClearActiveLineout();
    }
    private Team DetermineThrowingTeam(MatchManager manager)
    {

        if (lastTouchingTeam == manager.TeamA)
        {
            return manager.TeamB;
        }
        else
        {
            return manager.TeamA;
        }
    }
    private Vector3 CalculateLineoutPosition(Vector3 outPosition, LineoutConfig config)
    {
        float infieldX = SnapInfield(outPosition.x, config.touchlineOffset);
        return new Vector3(infieldX, outPosition.y, outPosition.z);
    }
    private float SnapInfield(float xPos, float offset)
    {
        if (xPos > 0)
        {
            return xPos - offset;
        }

        return xPos + offset;
    }

    private LineoutConfig GetConfig()
    {
        return MatchManager.Instance?.config?.lineoutConfig;
    }

    private void SubscribeToEvents()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnBallOutOfBounds += OnBallOutOfBounds;
    }

    private void UnsubscribeFromEvents()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnBallOutOfBounds -= OnBallOutOfBounds;
    }

    private void OnBallOutOfBounds(Vector3 position)
    {
        Team lastTouching = MatchManager.Instance?.PossessionTeam;
        RegisterOutOfBounds(position, lastTouching);
    }
}
