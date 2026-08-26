using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlPromptUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI actionLabel;   // e.g. "Pass Left"
    [SerializeField] private TextMeshProUGUI keyLabel;      // e.g. "Q"


    public void SetContent(string action, string key)
    {
        if (actionLabel != null) actionLabel.text = action;
        if (keyLabel != null) keyLabel.text = FormatKey(key);
    }

    private string FormatKey(string raw)
    {
        switch (raw)
        {
            case "LeftShift": return "L.SHIFT";
            case "RightShift": return "R.SHIFT";
            case "LeftControl": return "L.CTRL";
            case "RightControl": return "R.CTRL";
            case "LeftArrow": return "←";
            case "RightArrow": return "→";
            case "UpArrow": return "↑";
            case "DownArrow": return "↓";
            case "Space": return "SPACE";
            case "Tab": return "TAB";
            case "Return": return "ENTER";
            case "Comma": return ",";
            case "Period": return ".";
            default: return raw.ToUpper();
        }
    }
}