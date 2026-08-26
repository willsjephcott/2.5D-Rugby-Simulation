using UnityEngine;

[System.Serializable]
public class TeamKeybinds
{
    public string moveUp = "W";
    public string moveDown = "S";
    public string moveLeft = "A";
    public string moveRight = "D";
    public string sprint = "LeftShift";
    public string passLeft = "Q";
    public string passRight = "E";
    public string tackle = "Space";
    public string switchDefender = "Tab";
}

[System.Serializable]
public class KeybindData
{
    public TeamKeybinds teamA = new TeamKeybinds();
    public TeamKeybinds teamB = new TeamKeybinds();

    public static KeybindData CreateDefaults()
    {
        KeybindData data = new KeybindData();

        // Team A defaults
        data.teamA.moveUp = "W";
        data.teamA.moveDown = "S";
        data.teamA.moveLeft = "A";
        data.teamA.moveRight = "D";
        data.teamA.sprint = "LeftShift";
        data.teamA.passLeft = "Q";
        data.teamA.passRight = "E";
        data.teamA.tackle = "Space";
        data.teamA.switchDefender = "Tab";

        // Team B defaults
        data.teamB.moveUp = "UpArrow";
        data.teamB.moveDown = "DownArrow";
        data.teamB.moveLeft = "LeftArrow";
        data.teamB.moveRight = "RightArrow";
        data.teamB.sprint = "RightShift";
        data.teamB.passLeft = "Comma";
        data.teamB.passRight = "Period";
        data.teamB.tackle = "RightControl";
        data.teamB.switchDefender = "Aplha1";

        return data;
    }
}


