using UnityEngine;

public class NeedleController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public RectTransform safeZone;
    public SafeZoneRandomiser safeZoneRandomiser;

    public float moveSpeed = 200f;

    RectTransform pointerTransform;
    Vector3 targetPosition;
    IContest activeContest;
    bool isActive;

    /*private void OnDisable()
    {
        Debug.Log($"NeedleController: OnDisable called, stack={System.Environment.StackTrace}");
    }*/

    private void Awake()
    {
        pointerTransform = GetComponent<RectTransform>();
        //gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        MovePointer();
        CheckBoundsReached();
        CheckInput();
    }

    // Called by LineoutSlotSelectorUI : LineoutManager implements IKickContest
    public void Activate(IContest contest)
    {
        SetupContest(contest);
    }

    public void Deactivate()
    {
        isActive = false;
        transform.parent.gameObject.SetActive(false);
    }

    private void SetupContest(IContest contest)
    {
        //Debug.Log($"NeedleController: SetupContest called, parent={transform.parent?.name}, this.gameObject.activeSelf={gameObject.activeSelf}");
        activeContest = contest;
        isActive = true;
        targetPosition = pointB.position;
        safeZoneRandomiser.RandomisePosition();
        transform.parent.gameObject.SetActive(true);
        //Debug.Log($"NeedleController: After SetActive, parent.activeSelf={transform.parent?.gameObject.activeSelf}");
    }

    private void MovePointer()
    {
        pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void CheckBoundsReached()
    {
        if (Vector3.Distance(pointerTransform.position, pointA.position) < 0.1f) targetPosition = pointB.position;
        else if (Vector3.Distance(pointerTransform.position, pointB.position) < 0.1f) targetPosition = pointA.position;
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
            ResolveAttempt();
    }

    private void ResolveAttempt()
    {
        bool success = IsPointerInSafeZone();
        isActive = false;

        DebugLogResult(success);

        activeContest?.NotifyContestResult(success);
        activeContest = null;

        Deactivate();
    }

    private bool IsPointerInSafeZone()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(safeZone, pointerTransform.position, null);
    }

    private void DebugLogResult(bool success)
    {
        if (success)
        {
            Debug.Log("Needle QTE: Success");
        }
        else
        {
            Debug.Log("Needle QTE: Fail");
        }
    }
}