[System.Serializable]
public class LineoutConfig
{
    public int playersPerTeam = 4;          
    public float slotSpacing = 1.5f;        // Distance between slots along Z axis
    public float teamGap = 1.2f;            
    public float touchlineOffset = 2f;      // How far infield the lineout forms from the touchline
    public float throwDelay = 1.5f;         // Seconds before needle starts after slot selected
    public float needleSpeed = 1.8f;        
    public float needleZoneSize = 0.25f;    // Size of the success zone (0-1 range)
    public float joinMoveSpeed = 6f;        
    public float arrivalThreshold = 1f;   // Distance at which player is considered arrived
}