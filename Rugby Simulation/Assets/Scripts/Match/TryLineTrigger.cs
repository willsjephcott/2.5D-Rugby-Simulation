using UnityEngine;
using UnityEngine.SceneManagement;

public class TryLineTrigger : MonoBehaviour
{
    public Team scoringTeam;

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponentInParent<Ball>();
        if (ball == null) return;

        // Ball must be carried - loose ball doesn't count as a try
        if (ball.currentHolder == null) return;

        // Don't fire during a ruck
        if (ball.isInRuck) return;

        if (scoringTeam == null)
        {
            Debug.LogWarning($"TryLineTrigger on {gameObject.name}: scoringTeam is not assigned.");
            return;
        }

        ScoreManager.Instance?.RegisterTry(scoringTeam);
    }
}
