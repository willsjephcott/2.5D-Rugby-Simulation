using UnityEngine;

//Centralised gameplay tuning values

public class GameConfig : MonoBehaviour
{
    public RuckConfig ruckConfig = new RuckConfig();
    public LineoutConfig lineoutConfig = new LineoutConfig();
    public ConversionConfig conversionConfig = new ConversionConfig();

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;

    public float passSpeed = 30f;
    public float passArcHeight = 3f;
    public float passAnimationDelay = 0.15f;
    public float passStateExitTime = 0.35f;

    public Vector3 ballHoldOffset = new Vector3(0f, 1.2f, 0.5f);

    public float passBackwardDistance = 3f;
    public float passLateralDistance = 1.5f;

    public float greenMaxDistance = 5f;
    public float yellowMaxDistance = 12f;

    public float distanceWeight = 0.6f;
    public float handlingWeight = 0.25f; //Player skills are actually used
    public float difficultyWeight = 0.15f;

    public float distanceFalloffPower = 1.8f; // How quickly probability drops with distance (2 is closer to quadratic and 1 is linear)

    public float maxPassDistance = 25f;

    public float minProbability = 0.05f;
    public float maxProbability = 0.98f;

    public float passMissPerpendicularFactor = 0.15f;
    public float passMissLongitudinalFactor = 0.1f;

    public float defaultHandlingStat = 70f;

    //Defensive Config stuff
    public float smoothingFactor = 0.15f;
    public float rubberBandSpeed = 6f;
    public float arrivalThreshold;
    public float chaseActivationDistance = 6f;
    public int maxChasers = 2;
    public float chaseAngleThreshold;
    public float chaseDisengageDistance = 10f;
    public float chaseSpeedMultiplier = 1.3f; //Attached to maxDriftSpeed
    public float passReactionCooldown = 0.35f; //how long before line can fully reset after pass
    public float losseBallSpacingMultiplier = 0.6f; //When ball loose, line compresses
    public float rejoinSpeed = 4f;
    public float rejoinSnapDistance = 1f;
    public PassProbabilityAlgorithm CreateProbabilityAlgorithm()
    {
        return new PassProbabilityAlgorithm
        {
            greenMaxDistance = this.greenMaxDistance,
            yellowMaxDistance = this.yellowMaxDistance,
            distanceWeight = this.distanceWeight,
            handlingWeight = this.handlingWeight,
            difficultyWeight = this.difficultyWeight,
            distanceFalloffPower = this.distanceFalloffPower,
            maxPassDistance = this.maxPassDistance,
            minProbability = this.minProbability,
            maxProbability = this.maxProbability
        };
    }
}

