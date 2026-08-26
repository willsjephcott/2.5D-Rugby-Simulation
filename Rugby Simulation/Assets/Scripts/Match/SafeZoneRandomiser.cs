using UnityEngine;

public class SafeZoneRandomiser : MonoBehaviour
{
    public RectTransform safeZone;
    public float randomRangeX = 200f;

    public void RandomisePosition()
    {
        if (safeZone == null) return;
        safeZone.anchoredPosition = new Vector2(
            Random.Range(-randomRangeX, randomRangeX),safeZone.anchoredPosition.y);
    }
}