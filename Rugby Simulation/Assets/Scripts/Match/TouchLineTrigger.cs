using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponentInParent<Ball>();
        if (ball == null)
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.hasBall)
            {
                ball = MatchManager.Instance?.ball;
            }
        }
        if (ball == null) return;
        if (ball.isInRuck) return;
        if (ball.isInLineout) return;

        Vector3 outPosition = ball.transform.position;

        Debug.Log($"Ball out of bounds at {outPosition} via {gameObject.name}");

        // Hook for lineout logic later — subscribe to this from a LineoutManager
        bool halfTimeTriggered = MatchTimerManager.Instance?.IsInExtraTime == true;
        MatchTimerManager.Instance?.NotifyDeadBall();
        if (!halfTimeTriggered) ScoreManager.Instance?.RegisterOutOfBounds(outPosition);
    }
}