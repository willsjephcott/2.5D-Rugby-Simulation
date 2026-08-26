using UnityEngine;

[System.Serializable]
public class RuckConfig
{
    public int supportPlayersPerTeam = 2;
    public float stackSpacing = 0.8f;  // Depth between players in stack
    public float stackOffset = 0.4f;  // Lateral offset so players are visible
    public float unloadInterval = 0.5f;  // Seconds between each player releasing
    public float scrumhalfDistance = 1.5f;  // How far behind ruck the scrumhalf stands

}
