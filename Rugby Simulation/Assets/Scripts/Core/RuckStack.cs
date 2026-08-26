using UnityEngine;
using System.Collections.Generic;

public class RuckStack
{
    Vector3 ruckPosition;
    Vector3 stackDirection;
    float lateralOffset;

    Stack<Player> stack = new Stack<Player>();

    bool isUnloading;
    float unloadInterval;
    float unloadTimer;

    public RuckStack(Vector3 ruckPosition, Vector3 stackDirection, float lateralOffset)
    {
        this.ruckPosition = ruckPosition;
        this.stackDirection = stackDirection.normalized;
        this.lateralOffset = lateralOffset;
    }

    public void Push(Player player)
    {
        stack.Push(player);
    }
    public void BeginUnloading(float interval)
    {
        isUnloading = true;
        unloadInterval = interval;
        unloadTimer = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (!isUnloading) return;
        if (IsEmpty()) return;

        unloadTimer += deltaTime;

        if (HasReachedUnloadInterval())
        {
            UnloadTopPlayer();
            ResetUnloadTimer();
        }
    }
    public bool IsEmpty()
    {
        return stack.Count == 0;
    }

    public int Count()
    {
        return stack.Count;
    }

    private bool HasReachedUnloadInterval()
    {
        return unloadTimer >= unloadInterval;
    }

    private void UnloadTopPlayer()
    {
        Player player = stack.Pop();
        ReleasePlayer(player);
    }
    private void ReleasePlayer(Player player)
    {
        if (player == null) return;
        player.stateMachine.SM.SetState(new AIIdleState(player.stateMachine));
    }

    private void ResetUnloadTimer()
    {
        unloadTimer = 0f;
    }
}
