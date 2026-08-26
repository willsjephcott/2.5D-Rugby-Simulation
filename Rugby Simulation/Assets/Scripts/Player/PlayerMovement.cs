using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Handles player movement (doesn't decide when or why)
    Player player;
    GameConfig gameConfig;

    Vector3 pitchUp = Vector3.right;   // +X
    Vector3 pitchAcross = Vector3.forward; // +Z

    const float StaminaDrainRate = 8f;
    const float StaminaRegenRate = 5f;
    const float SpeedStatMax = 8f;
    const float SpeedStatMin = 4f;
    public void Initialise(Player player)
    {
        this.player = player;
        gameConfig = FindConfig();
    }
    private GameConfig FindConfig()
    {
        if (MatchManager.Instance != null)
        {
            return MatchManager.Instance.config;
        }
        return FindAnyObjectByType<GameConfig>();
    }
    public void Move(Vector2 input, bool isSprinting)
    {
        if (!ValidateMovement()) return;

        UpdateStamina(isSprinting);

        float speed = CalculateSpeed(isSprinting);
        Vector3 direction = CalculateMovementDirection(input);

        ExecutePhysicsMovement(direction, speed);
        UpdateSpriteDirection(input);
    }
    private void UpdateStamina(bool isSprinting)
    {
        if (isSprinting && !IsStaminaDepleted())
        {
            player.currentStamina -= StaminaDrainRate * Time.fixedDeltaTime;
            player.currentStamina = Mathf.Max(0f, player.currentStamina);
        }
        else if (!isSprinting)
        {
            player.currentStamina += StaminaRegenRate * Time.fixedDeltaTime;
            player.currentStamina = Mathf.Min(player.stats.stamina, player.currentStamina);
        }   
    }
    private bool IsStaminaDepleted()
    {
        return player.currentStamina <= 0f;
    }
    private float SprintSpeedFromStat()
    {
        float t = player.stats.speed / 100f;
        return Mathf.Lerp(SpeedStatMin, SpeedStatMax, t);
    }
    private Vector3 CalculateMovementDirection(Vector2 input)
    {
        // input.y controls movement up/down the pitch (right direction in world)
        // input.x controls movement left/right across pitch (forward direction in world)
        Vector3 direction = new Vector3(input.y, 0f, -input.x);

        return NormalizeDirection(direction);
    }
    private Vector3 NormalizeDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }
        return direction;
    }
    private float CalculateSpeed(bool isSprinting)
    {
        bool canSprint = isSprinting && !IsStaminaDepleted();

        if (canSprint)
        {
            return SprintSpeedFromStat();
        }

        return gameConfig.walkSpeed;
    }
    private void ExecutePhysicsMovement(Vector3 direction, float speed)
    {
        Vector3 movement = CalculateMovementDelta(direction, speed);
        Vector3 newPosition = CalculateNewPosition(movement);

        ApplyMovement(newPosition);
    }
    private Vector3 CalculateMovementDelta(Vector3 direction, float speed)
    {
        return direction * speed * Time.fixedDeltaTime;
    }
    private Vector3 CalculateNewPosition(Vector3 movement)
    {
        return player.rb.position + movement;
    }
    private void ApplyMovement(Vector3 newPosition)
    {
        player.rb.MovePosition(newPosition);
    }
    private void UpdateSpriteDirection(Vector2 input)
    {
        if (!ValidateSpriteRenderer()) return;

        if (ShouldFlipLeft(input))
        {
            FlipSpriteLeft();
        }
        else if (ShouldFlipRight(input))
        {
            FlipSpriteRight();
        }
    }
    private bool ShouldFlipLeft(Vector2 input)
    {
        return input.x < -0.01f;
    }
    private bool ShouldFlipRight(Vector2 input)
    {
        return input.x > 0.01f;
    }
    private void FlipSpriteLeft()
    {
        player.sr.flipX = false;
    }
    private void FlipSpriteRight()
    {
        player.sr.flipX = true;
    }
    private bool ValidateMovement()
    {
        if (player == null)
        {
            DebugMissingPlayer();
            return false;
        }

        if (player.rb == null)
        {
            DebugMissingRigidbody();
            return false;
        }

        if (gameConfig == null)
        {
            DebugMissingConfig();
            return false;
        }

        return true;
    }
    private bool ValidateSpriteRenderer()
    {
        return player.sr != null;
    }
    private void DebugMissingPlayer()
    {
        Debug.LogWarning("PlayerMovement: Missing player reference");
    }

    private void DebugMissingRigidbody()
    {
        Debug.LogWarning("PlayerMovement: Player is missing Rigidbody component");
    }

    private void DebugMissingConfig()
    {
        Debug.LogWarning("PlayerMovement: Missing GameConfig reference");
    }
    /*public void Initialise(Player player)
    {
        this.player = player;
        gameConfig = MatchManager.Instance != null ? MatchManager.Instance.config : FindAnyObjectByType<GameConfig>();
    }
    public void Move(Vector2 input, bool isSprinting)
    {
        if (player == null || player.rb == null || gameConfig == null) return;


        float speed = isSprinting ? gameConfig.sprintSpeed : gameConfig.walkSpeed;

        //Vector3 direction = new Vector3(input.x, 0f, input.y);
        Vector3 direction = new Vector3(input.y, 0f, -input.x);


        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        player.rb.MovePosition(player.rb.position + direction * speed * Time.fixedDeltaTime);

        // Update sprite facing (direction)
        if (player.sr != null)
        {
            if (input.x < -0.01f) player.sr.flipX = false;
            if (input.x > 0.01f) player.sr.flipX = true;
        }
    }*/
}
