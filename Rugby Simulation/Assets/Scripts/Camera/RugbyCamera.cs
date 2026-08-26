using UnityEngine;

public class RugbyCamera : MonoBehaviour
{
    public Transform target;
    public float sideLineDistance = -15f;
    public float height = 8f;
    public float lookAheadDistance = 3f;

    //Camera limits
    public float minX = -40f;
    public float maxX = 40f;

    public float followSpeed = 8f;
    public float tiltAngle = 10f;

    public float orthographicSize = 6f;
    public float fieldOfView = 50f;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam.orthographic)
        {
            cam.orthographicSize = orthographicSize;
        }
        else
        {
            cam.fieldOfView = fieldOfView;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("RugbyCamera has no target assigned");
            return;
        }

        Vector3 targetPosition = CalculateTargetPosition();

        //Uses critical damped spring model to move to a position (quickly then slowly as you get close)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.5f); //xf = follow speed

        transform.rotation = Quaternion.Euler(tiltAngle, 90, 0); //Quaternion allows rotation in 3D (Euler angle needs to be turned into it)
    }

    private Vector3 CalculateTargetPosition()
    {
        //add to look ahead slightly
        float targetZ = Mathf.Clamp(target.position.z + lookAheadDistance,minX,maxX);

        float targetX = target.position.x + sideLineDistance; ;

        return new Vector3(targetX, height, targetZ);
    }

    private void OnDrawGizmos()
    {
        // Draw pitch boundaries
        Gizmos.color = Color.yellow;
        Vector3 leftBound = new Vector3(minX, 0f, 0f);
        Vector3 rightBound = new Vector3(maxX, 0f, 0f);
        Gizmos.DrawLine(leftBound + Vector3.up * 10f, leftBound);
        Gizmos.DrawLine(rightBound + Vector3.up * 10f, rightBound);

        // Draw camera view line
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
