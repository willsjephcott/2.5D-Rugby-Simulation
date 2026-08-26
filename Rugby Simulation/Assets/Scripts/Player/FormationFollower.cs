using UnityEngine;

public class FormationFollower : MonoBehaviour
{
    private Player player;
    private PlayerMovement movement;
    private FormationSettings settings;

    private Vector3? targetPosition;
    private bool hasTarget;

    public void Initialise( Player player,FormationSettings settings)
    {
        this.player = player;
        movement = player.movement;
        this.settings = settings;

        ClearTarget();
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
    }
    public void ClearTarget()
    {
        targetPosition = null;
        hasTarget = false;
    }
    public bool HasTarget()
    {
        return hasTarget && targetPosition.HasValue;
    }
    public void FollowFormation()
    {
        if (!ValidateCanFollow())
        {
            return;
        }

        Vector3 directionToTarget = CalculateDirectionToTarget();
        float distanceToTarget = CalculateDistanceToTarget();

        if (HasArrivedAtTarget(distanceToTarget))
        {
            StopMovement();
            return;
        }

        MoveTowardTarget(directionToTarget);
    }
    private Vector3 CalculateDirectionToTarget()
    {
        Vector3 direction = targetPosition.Value - player.transform.position;
        direction.y = 0;
        return direction.normalized;
    }
    private float CalculateDistanceToTarget()
    {
        Vector3 horizontalDifference = targetPosition.Value - player.transform.position;
        horizontalDifference.y = 0;
        return horizontalDifference.magnitude;
    }
    private bool HasArrivedAtTarget(float distance)
    {
        return distance < settings.arrivalThreshold;
    }
    private void MoveTowardTarget(Vector3 direction)
    {
        if (movement == null) return;

        Vector2 moveInput = ConvertDirectionToInput(direction); //convert 3D direction to 2D input
        bool sprint = false;
        movement.Move(moveInput, sprint);
    }
    private Vector2 ConvertDirectionToInput(Vector3 direction)
    {
        //PlayerMovement uses input.y = up/down (x) and input.x = left/right (z)
        // direction.x right
        // direction.z forward
        return new Vector2(-direction.z, direction.x);
    }
    private void StopMovement()
    {
        if (movement != null)
        {
            movement.Move(Vector2.zero, false);
        }
    }
    private bool ValidateCanFollow()
    {
        if(!hasTarget || !targetPosition.HasValue) return false;
        if (player == null)
        {
            DebugLogMissingPlayer();
            return false;
        }
        if (movement == null)
        {
            DebugLogMissingMovement();
            return false;
        }
        return true;
    }
    private void DebugLogMissingPlayer()
    {
        Debug.LogWarning("FormationFollower: Missing player reference");
    }

    private void DebugLogMissingMovement()
    {
        Debug.LogWarning("FormationFollower: Player missing PlayerMovement component");
    }

    public void DebugDrawTarget()
    {
        if (!hasTarget || !targetPosition.HasValue) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPosition.Value, 0.5f);
        Gizmos.DrawLine(player.transform.position, targetPosition.Value);
    }
}
