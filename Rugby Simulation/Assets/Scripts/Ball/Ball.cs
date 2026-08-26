using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class Ball : MonoBehaviour
{
    public Transform currentHolder { get; private set; }

    public Rigidbody rb;
    public PassHandler passHandler;
    public bool isInRuck { get; private set; }
    public bool isInLineout { get; private set; }

    GameConfig config;


    public event Action<Transform, Transform> OwnerChanged; // (old, new)
    public Ball(GameConfig config)
    {
        this.config = config;
    }

    private void Awake()
    {
        InitialiseComponents();
        InitialiseConfig();
        InitialisePassHandler();
    }

    private void Update()
    {
        UpdateBallPosition();
        UpdatePhysicsState();
    }
    public void SetRuckLock(bool locked)
    {
        isInRuck = locked;
    }
    public void SetLineoutLock(bool locked)
    {
        isInLineout = locked;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanBePickedUp()) return;

        Player player = FindPlayerInCollision(collision);
        if(player != null)
        {
            AttachTo(player.transform);
        }
    }

    private void InitialiseComponents()
    {
        rb = GetComponent<Rigidbody>();
        passHandler = GetComponent<PassHandler>();
    }
    private void InitialiseConfig()
    {
        config = FindConfig();

        if (!ValidateConfig())
        {
            DebugMissingConfig();
        }
    }
    private GameConfig FindConfig()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.config != null)
        {
            return MatchManager.Instance.config;
        }
        return FindAnyObjectByType<GameConfig>();
    }
    private void InitialisePassHandler()
    {
        if (passHandler != null)
        {
            passHandler.Initialise(this, config);
        }
    }

    private void UpdateBallPosition()
    {
        if (IsBeingHeld() && !IsBeingPassed())
        {
            PositionBallOnHolder();
        }
    }
    private void PositionBallOnHolder()
    {
        if (config == null) return;

        transform.position = currentHolder.position + config.ballHoldOffset;
    }
    private bool IsBeingHeld()
    {
        return currentHolder != null;
    }

    private bool IsBeingPassed()
    {
        return passHandler != null && passHandler.IsPassing();
    }
    private void UpdatePhysicsState()
    {
        if (IsBeingHeld() && !IsBeingPassed() && !rb.isKinematic)
        {
            rb.isKinematic = true;
        }
        else if (!IsBeingHeld() && !IsBeingPassed() && rb.isKinematic)
        {
            rb.isKinematic = false;
        }
    }
    public void AttachTo(Transform holder)
    {
        if (!ValidateHolder(holder)) return;

        Transform playerRoot = FindPlayerRoot(holder);
        currentHolder = playerRoot;
        rb.isKinematic = true;
        NotifyOwnershipChange(currentHolder, playerRoot);
    }
    public void Drop()
    {
        Transform previousHolder = currentHolder;
        currentHolder = null;
        NotifyOwnershipChange(previousHolder, null);
    }
    private Transform FindPlayerRoot(Transform holder)
    {
        Player player = holder.GetComponentInParent<Player>();
        if (player != null)
        {
            return player.transform;
        }

        return holder;
    }
    private void NotifyOwnershipChange(Transform oldHolder, Transform newHolder)
    {
        OwnerChanged?.Invoke(oldHolder, newHolder);
    }

    private bool CanBePickedUp()
    {
        return !IsBeingHeld() && !IsBeingPassed() && !isInRuck && !isInLineout;
    }
    private Player FindPlayerInCollision(Collision collision)
    {
        return collision.collider.GetComponentInParent<Player>();
    }
    private bool ValidateHolder(Transform holder)
    {
        return holder != null;
    }
    private bool ValidateConfig()
    {
        return config != null;
    }
    private void DebugMissingConfig()
    {
        Debug.LogWarning("Ball is missing GameConfig; assign one in the scene.");
    }

}    /*private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        passHandler = GetComponent<PassHandler>();

        if (config == null)
        {
            config = MatchManager.Instance?.config;
        }

        if (config == null)
        {
            config = FindAnyObjectByType<GameConfig>();
        }

        if (config == null)
        {
            Debug.LogWarning("Ball is missing GameConfig; assign one in the scene.");
        }

        if (passHandler != null)
        {
            passHandler.Initialise(this, config);
        }
    }

    private void Update()
    {
        // If held, follow holder
        if (currentHolder != null && !passHandler.IsPassing())
        {
            if (!rb.isKinematic)
            {
                rb.isKinematic = true;
            }

            transform.position = currentHolder.position + config.ballHoldOffset;
        }
        else if (currentHolder == null && !passHandler.IsPassing())
        {
            // Loose ball - enable physics
            if (rb.isKinematic)
                rb.isKinematic = false;
        }
    }

    public void AttachTo(Transform holder)
    {
        if (holder == null) return;
        var player = holder.GetComponentInParent<Player>();
        Transform playerRoot = player != null ? player.transform : holder;

        var oldHolder = currentHolder;
        currentHolder = playerRoot;

        //rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        OwnerChanged?.Invoke(oldHolder, holder);
    }

    public void Drop()
    {
        var oldHolder = currentHolder;
        currentHolder = null;

        OwnerChanged?.Invoke(oldHolder, null);
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Only pick up if ball is loose and not being passed
        if (currentHolder != null || passHandler.IsPassing()) return;

        var player = collision.collider.GetComponentInParent<Player>();
        if (player != null)
        {
            AttachTo(player.transform);
        }
    }*/

