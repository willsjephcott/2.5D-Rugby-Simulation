using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [Range(0f, 100f)] public float handling;
    [Range(0f, 100f)] public float speed;
    [Range(0f, 100f)] public float stamina;
    [Range(0f, 100f)] public float aggression;
}
// Core Player Data and references
public class Player : MonoBehaviour
{
    public Team team;

    public bool isControlled { get; private set; }
    public bool hasBall { get; private set; }


    public PlayerGroup playerGroup;

    public PlayerStats stats = new PlayerStats();
    [HideInInspector] public float currentStamina;
    public float handlingStat // so that I don't have to edit PassProbabilityAlgorithm
    {
        get { return stats.handling; }
    }

    static readonly PlayerStats ForwardPreset = CreateForwardPreset();
    static readonly PlayerStats BackPreset = CreateBackPreset();

    private static PlayerStats CreateForwardPreset()
    {
        return new PlayerStats
        {
            handling = 55f,
            speed = 60f,
            stamina = 60f,
            aggression = 80f
        };
    }
    private static PlayerStats CreateBackPreset()
    {
        return new PlayerStats
        {
            handling = 80f,
            speed = 80f,
            stamina = 80f,
            aggression = 50f
        };
    }

    public Rigidbody rb { get; private set; }
    public SpriteRenderer sr { get; private set; }
    public Animator anim { get; private set; }

    public PlayerMovement movement { get; private set; }
    public PlayerAnimator animController { get; private set; }
    public PlayerStateMachine stateMachine { get; private set; }
    private void Awake()
    {
        ApplyGroupPreset();
        currentStamina = stats.stamina;
        InitialiseComponents();
        InitialiseSystems();
    }
    private void ApplyGroupPreset()
    {
        PlayerStats preset;

        if (playerGroup == PlayerGroup.Forward)
        {
            preset = ForwardPreset;
        }
        else
        {
            preset = BackPreset;
        }
        stats.handling = preset.handling;
        stats.speed = preset.speed;
        stats.stamina = preset.stamina;
        stats.aggression = preset.aggression;
    }
    private void InitialiseComponents()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    private void InitialiseSystems()
    {
        movement = GetComponent<PlayerMovement>();
        animController = GetComponent<PlayerAnimator>();
        stateMachine = GetComponent<PlayerStateMachine>();

        InitialiseMovementSystems();
        InitialiseAnimatorSystems();
        InitialiseStateMachineSystems();
    }
    private void InitialiseMovementSystems()
    {
        if (movement != null)
        {
            movement.Initialise(this);
        }
    }
    private void InitialiseAnimatorSystems()
    {
        if (animController != null)
        {
            animController.Initialise(this);
        }
    }
    private void InitialiseStateMachineSystems()
    {
        if (stateMachine != null)
        {
            stateMachine.Initialise(this);
        }
    }
    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
    }
    public void SetHasBall(bool hasBall)
    {
        this.hasBall = hasBall;
        NotifyAnimatorOfBallState(hasBall);
    }
    public void NotifyAnimatorOfBallState(bool hasBall)
    {
        animController?.UpdateBallState(hasBall);
    }

    /*public void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();

        movement = GetComponent<PlayerMovement>();
        animController = GetComponent<PlayerAnimator>();
        stateMachine = GetComponent<PlayerStateMachine>();

        if (movement != null) movement.Initialise(this);
        if (animController != null) animController.Initialise(this);
        if (stateMachine != null) stateMachine.Initialise(this);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
    }
    public void SetHasBall(bool hasBall)
    {
        this.hasBall = hasBall;
        animController?.UpdateBallState(hasBall);
    }*/
}
