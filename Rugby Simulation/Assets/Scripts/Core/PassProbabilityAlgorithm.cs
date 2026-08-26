using Unity.Collections;
using UnityEngine;

public class PassProbabilityAlgorithm
{
    public float greenMaxDistance = 5f;
    public float yellowMaxDistance = 12f;

    public float distanceWeight = 0.6f;
    public float handlingWeight = 0.25f; //Player skills are actually used
    public float difficultyWeight = 0.15f;

    public float distanceFalloffPower = 1.8f; // How quickly probability drops with distance (2 is closer to quadratic and 1 is linear)

    public float maxPassDistance = 25f;

    public float minProbability = 0.05f;
    public float maxProbability = 0.98f;

    public (float probability, PassZone zone) CalculatePassProbability(float distance, float handlingStat, float difficultyModifier = 1f)
    {
        float normalisedHandling = Mathf.Clamp01(handlingStat / 100f); //(0-1)
        float distanceFactor = CalculateDistanceFactor(distance); //(0-1)
        float handlingFactor = CalculateHandlingFactor(normalisedHandling); //(0-1)
        float difficultyFactor = CalculateDifficultyFactor(difficultyModifier); //  0.5 -> 1, 1-> 0.67, 2 -> 0.4 (makes it easier with the formula applied instead of 0.75,0.5,0.25

        float rawProbability = distanceFactor * distanceWeight + handlingFactor * handlingWeight + difficultyFactor * difficultyWeight;
        float finalProbability = Mathf.Clamp(rawProbability, minProbability, maxProbability);

        PassZone zone = GetZoneForDistance(distance);

        return (finalProbability, zone);
    }

    public PassZone GetZoneForDistance(float distance)
    {
        if (distance <= greenMaxDistance) return PassZone.Green;
        else if (distance <= yellowMaxDistance) return PassZone.Yellow;
        else return PassZone.Red;
    }

    //Calculates the expected miss radius if the pass fails
    public float CalculateMissRadius(float probability, float distance)
    {
        float baseRadius = distance * 0.15f; //Distance it will miss from base

        float inverseFactor = Mathf.Lerp(5f, 1.2f, probability); // p =0.9 -> x1.2 , p=0.1 -> x5

        return baseRadius * inverseFactor;
    }

    //Uses falloff curve so longer passes are exponentially harder
    private float CalculateDistanceFactor(float distance)
    {
        float normalisedDistance = Mathf.Clamp01(distance / maxPassDistance);

        float factor = 1f - Mathf.Pow(normalisedDistance, distanceFalloffPower); // creates the exponential curve


        return Mathf.Clamp01(factor);
    }

    private float CalculateHandlingFactor(float normalisedHandling)
    {
        float factor = Mathf.Pow(normalisedHandling, 0.85f); // 0.5 handling -> 0.55 factor, 0.9 handling -> 0.91 factor

        return Mathf.Clamp01(factor);
    }

    private float CalculateDifficultyFactor(float difficultyModifier)
    {
        // When modifier = 0.5 -> 1/(0.5+0.5) = 1.0
        // When modifier = 1.0 -> 1/(0.5+1) = 0.667
        // When modifier = 2.0 -> 1/(0.5+2) = 0.4
        float factor = 1f / (0.5f + difficultyModifier);

        return Mathf.Clamp01(factor);
    }

    //Pass difficulty zone based on distance
    
}
public enum PassZone
{
    Green, Yellow, Red
}
