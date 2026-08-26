using UnityEngine;

[System.Serializable]
public class FormationSettings
{
    public int numberOfLanes = 4;
    public float firstLaneDepth = 3f;
    public float depthIncrement = 2.5f;
    public float forwardBaseWidth = 3.5f;
    public float backBaseWidth = 4.5f;
    public float widthIncrement = 2.5f;
    public float formationFollowSpeed = 4f;
    public float arrivalThreshold = 0.3f;
    public bool forwardsInsideLanes = true;
}
