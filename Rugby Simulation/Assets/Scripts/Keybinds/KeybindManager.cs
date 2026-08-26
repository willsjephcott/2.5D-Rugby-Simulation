using UnityEngine;
using System.IO;

public class KeybindManager
{
    public static KeybindData Current { get; private set; }

    const string FileName = "keybinds.json";

    static KeybindManager()
    {
        LoadOrCreateDefaults();
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(Current, prettyPrint: true);
        File.WriteAllText(GetFilePath(), json);
        DebugLogSaved();
    }
    public static void ResetToDefaults()
    {
        Current = KeybindData.CreateDefaults();
        Save();
        DebugLogReset();
    }

    public static bool IsKeyDown(string keyName)
    {
        if (!TryParseKey(keyName, out KeyCode key)) return false; //Tries to convert keyname into keycode (input space output keycode.space)
        return Input.GetKeyDown(key);
    }

    public static bool IsKeyHeld(string keyName)
    {
        if (!TryParseKey(keyName, out KeyCode key)) return false;
        return Input.GetKey(key);
    }
    public static TeamKeybinds GetTeamBinds(bool isTeamA)
    {
        if (isTeamA)
        {
            return Current.teamA;
        }
        else
        {
            return Current.teamB;
        }
    }

    public static void SetBinding(bool isTeamA, string actionName, string newKey)
    {
        TeamKeybinds binds = GetTeamBinds(isTeamA);
        SetActionKey(binds, actionName, newKey);
        Save();
        DebugLogBindingChanged(isTeamA, actionName, newKey);
    }
    private static void LoadOrCreateDefaults()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            Current = JsonUtility.FromJson<KeybindData>(File.ReadAllText(path));
            DebugLogLoaded(path);
            return;
        }

        Current = KeybindData.CreateDefaults();
        Save();
        DebugLogCreatedDefaults(path);
    }
    private static void SetActionKey(TeamKeybinds binds, string actionName, string newKey)
    {
        switch (actionName)
        {
            case "moveUp":
                binds.moveUp = newKey; break;
            case "moveDown":
                binds.moveDown = newKey; break;
            case "moveLeft":
                binds.moveLeft = newKey; break;
            case "moveRight":
                binds.moveRight = newKey; break;
            case "sprint":
                binds.sprint = newKey; break;
            case "passLeft":
                binds.passLeft = newKey; break;
            case "passRight":
                binds.passRight = newKey; break;
            case "tackle":
                binds.tackle = newKey; break;
            case "switchDefender":
                binds.switchDefender = newKey; break;
            default:
                DebugLogUnknownAction(actionName); break;
        }
    }
    private static bool TryParseKey(string keyName, out KeyCode key)
    {
        if (System.Enum.TryParse(keyName, out key)) return true; //Tryparse is a premade enum
        DebugLogInvalidKey(keyName);
        return false;
    }

    private static string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    private static void DebugLogSaved()
    {
        Debug.Log($"KeybindManager: Saved to {GetFilePath()}");
    }

    private static void DebugLogLoaded(string path)
    {
        Debug.Log($"KeybindManager: Loaded from {path}");
    }

    private static void DebugLogCreatedDefaults(string path)
    {
        Debug.Log($"KeybindManager: Created defaults at {path}");
    }

    private static void DebugLogReset()
    {
        Debug.Log("KeybindManager: Reset to defaults.");
    }

    private static void DebugLogBindingChanged(bool isTeamA, string action, string key)
    {
        string team;

        if (isTeamA)
        {
            team = "Team A";
        }
        else
        {
            team = "Team B";
        }
        Debug.Log($"KeybindManager: {team} {action} rebound to {key}");
    }

    private static void DebugLogInvalidKey(string keyName)
    {
        Debug.LogWarning($"KeybindManager: '{keyName}' is not a valid KeyCode.");
    }

    private static void DebugLogUnknownAction(string actionName)
    {
        Debug.LogWarning($"KeybindManager: Unknown action '{actionName}'");
    }
}
