using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeybindRowUI : MonoBehaviour 
{
    public TextMeshProUGUI actionLabel;
    public Button keyButton;
    public TextMeshProUGUI keyButtonText;

    bool isTeamA;
    string actionName;
    bool isListening;

    public void Initialise(bool isTeamA, string actionName, string displayName)
    {
        this.isTeamA = isTeamA;
        this.actionName = actionName;

        if (actionLabel != null) actionLabel.text = displayName;

        SetupButton();
        RefreshKeyDisplay();
    }

    private void Update()
    {
        if (!isListening) return;
        DetectKeypress();
    }

    private void SetupButton()
    {
        if (keyButton == null) return;
        keyButton.onClick.RemoveAllListeners();
        keyButton.onClick.AddListener(OnKeyButtonPressed);
    }
    private void OnKeyButtonPressed()
    {
        isListening = true;
        if (keyButtonText != null) keyButtonText.text = "...";
    }

    private void DetectKeypress()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key)) continue;
            if (key == KeyCode.Escape)
            {
                CancelListening();
                return;
            }
            ApplyNewBinding(key.ToString());
            return;
        }
    }
    private void ApplyNewBinding(string keyName)
    {
        KeybindManager.SetBinding(isTeamA, actionName, keyName);
        isListening = false;
        RefreshKeyDisplay();
    }

    private void CancelListening()
    {
        isListening = false;
        RefreshKeyDisplay();
    }

    private void RefreshKeyDisplay()
    {
        if (keyButtonText == null) return;
        TeamKeybinds binds = KeybindManager.GetTeamBinds(isTeamA);
        keyButtonText.text = GetCurrentKey(binds);
    }
    private string GetCurrentKey(TeamKeybinds binds)
    {
        switch (actionName)
        {
            case "moveUp":
                return binds.moveUp;
            case "moveDown":
                return binds.moveDown;
            case "moveLeft":
                return binds.moveLeft;
            case "moveRight":
                return binds.moveRight;
            case "sprint":
                return binds.sprint;
            case "passLeft":
                return binds.passLeft;
            case "passRight":
                return binds.passRight;
            case "tackle":
                return binds.tackle;
            case "switchDefender":
                return binds.switchDefender;
            default:
                return "?";
        }
    }
}
