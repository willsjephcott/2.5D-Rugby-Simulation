using UnityEngine;
using UnityEngine.UI;

public class KeybindUI : MonoBehaviour
{
    public GameObject panel;
    public Transform teamARowContainer;
    public Transform teamBRowContainer;
    public Button resetButton;
    public Button closeButton;

    public KeybindRowUI rowPrefab;

    bool isOpen;

    static readonly (string action, string label)[] Actions =
    {
        ("moveUp",    "Move Up"),
        ("moveDown",  "Move Down"),
        ("moveLeft",  "Move Left"),
        ("moveRight", "Move Right"),
        ("sprint",    "Sprint"),
        ("passLeft",  "Pass Left"),
        ("passRight", "Pass Right"),
        ("tackle",    "Tackle"),
        ("switchDefender", "Switch Defender"),
    };
    private void Awake()
    {
        Hide();
        SetupButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();
    }

    public void Show()
    {
        isOpen = true;
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
        BuildRows();
    }

    public void Hide()
    {
        isOpen = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ToggleMenu()
    {
        if (isOpen) Hide();
        else Show();
    }
    private void SetupButtons()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetPressed);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void BuildRows()
    {
        if (!ValidateRowPrefab()) return;

        ClearRows(teamARowContainer);
        ClearRows(teamBRowContainer);

        SpawnRows(isTeamA: true, container: teamARowContainer); //teamA is true and use teamARowContainers
        SpawnRows(isTeamA: false, container: teamBRowContainer);
    }
    private void SpawnRows(bool isTeamA, Transform container)
    {
        if (container == null) return;

        foreach (var (action, label) in Actions)
        {
            KeybindRowUI row = Instantiate(rowPrefab, container);
            row.Initialise(isTeamA, action, label);
        }
    }

    private void ClearRows(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private void OnResetPressed()
    {
        KeybindManager.ResetToDefaults();
        BuildRows();
    }

    private bool ValidateRowPrefab()
    {
        if (rowPrefab != null) return true;
        DebugLogMissingPrefab();
        return false;
    }

    private void DebugLogMissingPrefab()
    {
        Debug.LogWarning("KeybindUI: rowPrefab is not assigned.");
    }
}
