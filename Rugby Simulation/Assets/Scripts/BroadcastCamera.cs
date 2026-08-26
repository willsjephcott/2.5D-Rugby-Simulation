using UnityEngine;

public class BroadcastCamera : MonoBehaviour
{
    public Transform ball;          

    public float forwardOffset = -40f; // How far from sideline (x-axis)
    public float height = 20f;      // Camera height
    public float minX = -50f;       // Limits along pitch
    public float maxX = 50f;

    public float followSpeed = 5f;

    public float lookHeightOffset = 1.5f; // Look slightly above ball

    void LateUpdate()
    {
        if (ball == null) return;

        // 1. Clamp camera movement along pitch (X axis)
        float targetX = Mathf.Clamp(ball.position.x, minX, maxX);

        // 2. Fixed side camera position (broadcast) + follow on X
        Vector3 targetPosition = new Vector3(targetX, height, forwardOffset);

        // 3. Smooth move
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // 4. Make the camera look at the ball 
        Vector3 lookTarget = ball.position + Vector3.up * lookHeightOffset;
        transform.LookAt(lookTarget);
    }
}
