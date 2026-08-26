using UnityEngine;

// Handles pass trajectory and animation
public class PassHandler : MonoBehaviour
{
    Ball ball;
    GameConfig config;
    PassProbabilityAlgorithm probabilityAlgorithm;

    bool isPassing;
    Transform passTargetPlayer;
    bool passToGround;

    Vector3 startPos;
    float progress;
    float duration;

    bool passWasSuccessful;
    Vector3 actualTargetPos;

    public void Initialise(Ball ball, GameConfig config)
    {
        this.ball = ball;
        this.config = config;

        if (ValidateInitialisation())
        {
            probabilityAlgorithm = config.CreateProbabilityAlgorithm();
        }
    }
    private bool ValidateInitialisation()
    {
        if (ball == null)
        {
            DebugMissingBall();
            return false;
        }

        if (config == null)
        {
            DebugMissingConfig();
            return false;
        }

        return true;
    }
    private void Update()
    {
        if (isPassing)
        {
            UpdatePassAnimation();
        }
    }

    public bool IsPassing()
    {
        return isPassing;
    }
    public void ForcePassToTarget(Transform target)
    {
        if (target == null) return;

        ball.Drop();

        // Guarunteed Pass (for kick to goal)
        passTargetPlayer = null;
        passToGround = true;

        actualTargetPos = target.position;
        actualTargetPos.y = Mathf.Max(0, actualTargetPos.y);

        passWasSuccessful = true;   
        BeginPassAnimation(actualTargetPos);
    }
    public void RequestPassToPlayer(Transform target, float passerHandlingStat = 70f, float difficultyModifier = 1.0f)
    {
        if (!ValidatePassToPlayer(target)) return;

        ball.Drop();

        ConfigurePlayerPass(target);
        CalculatePassOutcome(target.position, passerHandlingStat, difficultyModifier);
        BeginPassAnimation(actualTargetPos);
    }

    public void RequestPassToGround(Vector3 targetPos, float passerHandlingStat = 70f, float difficultyModifier = 1.0f)
    {
        ball.Drop();

        ConfigureGroundPass(targetPos);
        CalculatePassOutcome(actualTargetPos, passerHandlingStat, difficultyModifier);
        BeginPassAnimation(actualTargetPos);
    }

    private void ConfigurePlayerPass(Transform target)
    {
        passTargetPlayer = target;
        passToGround = false;
    }

    private void ConfigureGroundPass(Vector3 targetPos)
    {
        passTargetPlayer = null;
        passToGround = true;

        //Ensure ground postion is valdi
        targetPos.y = Mathf.Max(0, targetPos.y);
        actualTargetPos = targetPos;
    }

    private void CalculatePassOutcome(Vector3 intendedTarget, float handlingStat, float difficultyModifier)
    {
        if (!ValidatePassProbabilitySystem())
        {
            ResolveAsSuccessfulPass(intendedTarget);
            return;
        }


        float distance = CalculatePassDistance(intendedTarget);
        bool success = RollForPassSuccess(distance, handlingStat, difficultyModifier, out float probability, out PassZone zone); // Outputs multiple results so the calculation only needs to be performed once (output parameters)

        if (success)
        {
            ResolveAsSuccessfulPass(intendedTarget);
            DebugPassSuccess(distance,probability, zone);
        }
        else
        {
            ResolveAsFailedPass(intendedTarget, distance,probability);
            DebugPassFailure(distance, probability, zone);
        }

        passWasSuccessful = success;
    }

    private float CalculatePassDistance(Vector3 intendedTarget)
    {
        return Vector3.Distance(ball.transform.position, intendedTarget);
    }

    private bool RollForPassSuccess(float distance, float handlingStat, float difficultyModifier, out float probability,out PassZone zone)
    {
        var result = probabilityAlgorithm.CalculatePassProbability(distance, handlingStat, difficultyModifier);
        probability = result.probability;
        zone = result.zone;

        float roll = Random.Range(0, 1f);
        return roll <= probability;
    }

    private void ResolveAsSuccessfulPass(Vector3 intendedTarget)
    {
        actualTargetPos = intendedTarget;
    }

    private void ResolveAsFailedPass(Vector3 intendedTarget, float distance, float probability)
    {
        actualTargetPos = CalculateMissPosition(intendedTarget, distance, probability);
    }

    private Vector3 CalculateMissPosition(Vector3 intendedTarget, float distance, float probability)
    {
        if (config == null) return intendedTarget;

        Vector3 passDirection = CalculatePassDirection(intendedTarget);
        Vector3 perpendicular = CalculatePerpendicular(passDirection);
        float missRadius = CalculateMissRadius(probability, distance);
        Vector3 missOffset = CalculateMissOffset(passDirection, perpendicular,  missRadius);

        return ApplyMissOffset(intendedTarget, missOffset);

    }

    private Vector3 CalculatePassDirection(Vector3 intendedTarget)
    {
        return (intendedTarget - ball.transform.position).normalized;
    }

    private Vector3 CalculatePerpendicular(Vector3 passDirection)
    {
        return Vector3.Cross(passDirection, Vector3.up).normalized;
    }

    private float CalculateMissRadius(float probability, float distance)
    {
        return probabilityAlgorithm.CalculateMissRadius(probability, distance);
    }

    private Vector3 CalculateMissOffset(Vector3 passDirection, Vector3 perpendicular, float missRadius)
    {
        float perpendicularError = Random.Range(-missRadius, missRadius)* config.passMissPerpendicularFactor;
        float longitudinalError = Random.Range(-missRadius, missRadius) * config.passMissLongitudinalFactor;

        return (perpendicular * perpendicularError) + (passDirection * longitudinalError);
    }
    private Vector3 ApplyMissOffset(Vector3 intendedTarget, Vector3 missOffset)
    {
        Vector3 missTarget = intendedTarget + missOffset;
        missTarget.y = Mathf.Max(0, missTarget.y);
        return missTarget;
    }

    private void BeginPassAnimation(Vector3 targetPos)
    {
        if (!ValidatePassAnimation()) return;

        InitialiseAnimationState(targetPos);
        EnablePassAnimation();
        PreparePhysicsForAnimation();
    }

    private void InitialiseAnimationState(Vector3 targetPos)
    {
        startPos = ball.transform.position;
        progress = 0;
        duration = CalculatePassDuration(targetPos);
    }

    private float CalculatePassDuration(Vector3 targetPos)
    {
        float distance = Vector3.Distance(startPos, targetPos);
        return Mathf.Max(distance / config.passSpeed, 0.01f);
    }

    private void EnablePassAnimation()
    {
        isPassing = true;
    }

    private void PreparePhysicsForAnimation()
    {
        ball.rb.isKinematic = true;
    }

    private void UpdatePassAnimation()
    {
        Vector3 currentTarget = CalculateCurrentTarget();
        float normalisedProgress = AdvanceAnimationProgress(); //Progress within 0-1

        UpdateBallPosition(currentTarget, normalisedProgress);

        if (IsAnimationComplete(normalisedProgress))
        {
            CompletePassAnimation();
        }
    }

    private Vector3 CalculateCurrentTarget()
    {
        if (ShouldTrackPlayerTarget())
        {
            return passTargetPlayer.position + config.ballHoldOffset;
        }
        else
        {
            Vector3 target = actualTargetPos;
            target.y = Mathf.Max(0, target.y);
            return target;
        }
    }

    private bool ShouldTrackPlayerTarget()
    {
        return !passToGround && passWasSuccessful && passTargetPlayer != null;
    }

    private float AdvanceAnimationProgress()
    {
        progress += Time.deltaTime;
        return Mathf.Clamp01(progress/duration);
    }

    private void UpdateBallPosition(Vector3 targetPos, float normalisedProgress)
    {
        Vector3 linearPosition = CalculateLinearPosition(targetPos,  normalisedProgress);
        float arcHeight = CalculateArcHeight(normalisedProgress);

        ball.transform.position = linearPosition+ Vector3.up * arcHeight;
    }

    private Vector3 CalculateLinearPosition(Vector3 targetPos, float normalisedProgress)
    {
        return Vector3.Lerp(startPos,targetPos, normalisedProgress);
    }

    private float CalculateArcHeight(float normalisedProgress)
    {
        return Mathf.Sin(normalisedProgress * Mathf.PI) * config.passArcHeight;
    }

    private bool IsAnimationComplete(float normalisedProgress)
    {
        return normalisedProgress >= 1f; //true if >= 1
    }

    private void CompletePassAnimation()
    {
        DisablePassAnimation();

        if (ShouldAttachToPlayer())
        {
            ball.AttachTo(passTargetPlayer);
        }
        else
        {
            DropBall();
        }
    }
    private void DisablePassAnimation()
    {
        isPassing = false;
    }
    private bool ShouldAttachToPlayer()
    {
        return !passToGround && passWasSuccessful && passTargetPlayer != null;
    }

    private void DropBall()
    {
        ball.transform.position = actualTargetPos;
        ball.rb.isKinematic = false;
    }

    //Validation

    private bool ValidatePassToPlayer(Transform target)
    {
        if (target == null)
        {
            DebugNullPassTarget();
            return false;
        }
        return true;
    }
    private bool ValidatePassProbabilitySystem()
    {
        if (probabilityAlgorithm == null || config == null)
        {
            DebugMissingProbabilitySystem();
            return false;
        }
        return true;
    }
    private bool ValidatePassAnimation()
    {
        if (ball == null || config == null)
        {
            DebugCannotStartPass();
            isPassing = false;
            return false;
        }
        return true;
    }


    //Debug Stuff
    private void DebugMissingBall()
    {
        Debug.LogWarning("PassHandler is missing its Ball reference.");
    }

    private void DebugMissingConfig()
    {
        Debug.LogWarning("PassHandler is missing GameConfig; assign one in the scene.");
    }

    private void DebugNullPassTarget()
    {
        Debug.LogWarning("PassHandler received null target for player pass.");
    }

    private void DebugMissingProbabilitySystem()
    {
        Debug.LogWarning("PassHandler missing probability algorithm, defaulting to success.");
    }

    private void DebugCannotStartPass()
    {
        Debug.LogWarning("PassHandler cannot start a pass without Ball and GameConfig references.");
    }

    private void DebugPassSuccess(float distance, float probability, PassZone zone)
    {
        Debug.Log($"Pass success - Distance: {distance}m, Prob: {probability}%, Zone: {zone}");
    }

    private void DebugPassFailure(float distance, float probability, PassZone zone)
    {
        Debug.Log($"Pass failed - Distance: {distance}m, Prob: {probability}%, Zone: {zone}");
    }

    /*private bool ResolvePassOutcome(float distance, float handlingStat, float difficultyModifier, Vector3 intendedTarget)
    {
        if (probabilityAlgorithm == null || config == null)
        {
            Debug.LogWarning("PassHandler missing prob algorithm so defaulting to success");
            actualTargetPos = intendedTarget;
            return true;
        }

        var (probability, zone) = probabilityAlgorithm.CalculatePassProbability(distance, handlingStat, difficultyModifier);

        float roll = Random.Range(0, 1f);
        bool success = roll <= probability;

        if (success)
        {
            actualTargetPos = intendedTarget;
            Debug.Log("Pass success Distance: {distance:F1}m, Prob: {probability:F0}%, Zone: {zone}, Roll: {roll:F2}");
        }
        else
        {
            actualTargetPos = CalculateMissTarget(intendedTarget, distance,probability);
            Debug.Log($"Pass failed Distance: {distance:F1}m, Prob: {probability:F0}%, Zone: {zone}, Roll: {roll:F2}");

        }
        return success;
    }*/

    /*private Vector3 CalculateMissTarget(Vector3 intendedTarget, float distance, float probability)
    {
        if (config == null) return intendedTarget;

        Vector3 passDirection = (intendedTarget-ball.transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(passDirection, Vector3.up).normalized;

        float missRadius = probabilityAlgorithm.CalculateMissRadius(probability, distance);

        float perpendicularError = Random.Range(-missRadius, missRadius) * config.passMissPerpendicularFactor;
        float longitudinalError = Random.Range(-missRadius, missRadius) * config.passMissLongitudinalFactor;

        Vector3 missOffset = (perpendicular*perpendicularError)+(passDirection*longitudinalError);

        Vector3 missTarget = intendedTarget + missOffset;
        missTarget.y = Mathf.Max(0f, missTarget.y);

        return missTarget;
    }*/
    /*public void BeginPass(Vector3 targetPos)
    {
        if (ball == null || config == null)
        {
            Debug.LogWarning("PassHandler cannot start a pass without Ball and GameConfig references.");
            isPassing = false;
            return;
        }



        startPos = ball.transform.position;
        progress = 0f;

        float distance = Vector3.Distance(startPos, targetPos);
        duration = Mathf.Max(distance/ config.passSpeed, 0.01f);

        isPassing = true;
        //ball.rb.linearVelocity = Vector3.zero;
        ball.rb.isKinematic = true;

    }*/

    /*private void AnimatePass()
    {
        Vector3 targetPos;

        if (!passToGround && passWasSuccessful && passTargetPlayer != null)
        {
            targetPos = passTargetPlayer.position + config.ballHoldOffset;
        }
        else
        {
            targetPos = actualTargetPos;
        }

        if (passToGround || !passWasSuccessful)
        {
            targetPos.y = Mathf.Max(0f, targetPos.y);
        }

        //Animating the Arc
        progress += Time.deltaTime;
        float t = Mathf.Clamp01(progress / duration); //Clamp01 returns a value between 0 and 1
        
        Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
        float arc = Mathf.Sin(t * Mathf.PI) * config.passArcHeight;

        ball.transform.position = linearPos + Vector3.up * arc;

        //Complete Pass
        if (t >= 1f)
        {
            isPassing = false;

            if (passToGround || !passWasSuccessful)
            {
                ball.transform.position = targetPos;
                ball.rb.isKinematic = false;
            }
            else if (passTargetPlayer != null && passWasSuccessful)
            {
                ball.AttachTo(passTargetPlayer);
            }
            else
            {
                //Target lost - Drop
                ball.rb.isKinematic = false;
            }
        }
    }*/
}
