using UnityEngine;
using static PassProbabilityAlgorithm;

public class PassIndicatorUI : MonoBehaviour
{
    public GameObject leftArrow;
    public GameObject rightArrow;
    public float forwardOffset = 0.5f;
    public float heightOffset = 1.5f;
    public float sideOffset = 2f;
    public Color greenColour = new Color(0.2f, 0.8f, 0.2f, 0.9f);
    public Color yellowColour = new Color(0.9f, 0.8f, 0.1f, 0.9f);
    public Color redColour = new Color(0.9f, 0.2f, 0.2f, 0.9f);

    private SpriteRenderer leftArrowSprite;
    private SpriteRenderer rightArrowSprite;
    private MeshRenderer leftArrowMesh;
    private MeshRenderer rightArrowMesh;
    private Transform currentPlayer;

    // World pitch axes:
    // Across the pitch (left/right) = Z axis
    // Up the pitch (forward/back)   = X axis
    private static readonly Vector3 PitchForward = Vector3.forward; // +Z
    private static readonly Vector3 PitchAcross = Vector3.right;   // +X

    private void Awake()
    {
        InitialiseArrowComponents();
        HideAll();
    }
    private void LateUpdate()
    {
        if (currentPlayer != null)
        {
            UpdateArrowPositions();
        }
    }
    private void InitialiseArrowComponents()
    {
        InitialiseLeftArrowComponents();
        InitialiseRightArrowComponents();
    }
    private void InitialiseLeftArrowComponents()
    {
        if (leftArrow == null) return;
        leftArrowSprite = leftArrow.GetComponent<SpriteRenderer>();
        leftArrowMesh = leftArrow.GetComponent<MeshRenderer>();
    }
    private void InitialiseRightArrowComponents()
    {
        if (rightArrow == null) return;
        rightArrowSprite = rightArrow.GetComponent<SpriteRenderer>();
        rightArrowMesh = rightArrow.GetComponent<MeshRenderer>();
    }
    public void SetFollowTarget(Transform player)
    {
        currentPlayer = player;
        if (player == null) HideAll();
    }
    private void UpdateArrowPositions()
    {
        if (currentPlayer == null) return;

        if (IsArrowVisible(leftArrow)) UpdateLeftArrowPosition();
        if (IsArrowVisible(rightArrow)) UpdateRightArrowPosition();
    }
    private bool IsArrowVisible(GameObject arrow)
    {
        return arrow != null && arrow.activeSelf;
    }
    private void UpdateLeftArrowPosition()
    {
        leftArrow.transform.position = CalculateLeftArrowPosition();
    }
    private void UpdateRightArrowPosition()
    {
        rightArrow.transform.position = CalculateRightArrowPosition();
    }

    // Left = negative Z (across pitch), slightly forward up the pitch
    private Vector3 CalculateLeftArrowPosition()
    {
        Vector3 pos = currentPlayer.position;
        pos -= PitchAcross * sideOffset;
        pos += PitchForward * forwardOffset;
        pos += Vector3.up * heightOffset;
        return pos;
    }

    // Right = positive Z (across pitch), slightly forward up the pitch
    private Vector3 CalculateRightArrowPosition()
    {
        Vector3 pos = currentPlayer.position;
        pos += PitchAcross * sideOffset;
        pos += PitchForward * forwardOffset;
        pos += Vector3.up * heightOffset;
        return pos;
    }

    public void ShowLeft(PassZone zone, float probability = 0f)
    {
        if (leftArrow == null) return;
        leftArrow.SetActive(true);
        SetArrowColour(leftArrowSprite, leftArrowMesh, zone);
    }
    public void ShowRight(PassZone zone, float probability = 0f)
    {
        if (rightArrow == null) return;
        rightArrow.SetActive(true);
        SetArrowColour(rightArrowSprite, rightArrowMesh, zone);
    }
    public void HideLeft()
    {
        if (leftArrow != null) leftArrow.SetActive(false);
    }
    public void HideRight()
    {
        if (rightArrow != null) rightArrow.SetActive(false);
    }
    public void HideAll()
    {
        HideLeft();
        HideRight();
    }
    private void SetArrowColour(SpriteRenderer spriteComponent, MeshRenderer meshComponent, PassZone zone)
    {
        Color targetColour = GetColourForZone(zone);
        ApplyColourToSprite(spriteComponent, targetColour);
        ApplyColourToMesh(meshComponent, targetColour);
    }
    private Color GetColourForZone(PassZone zone)
    {
        return zone switch
        {
            PassZone.Green => greenColour, // if passzone.green it becomes green
            PassZone.Yellow => yellowColour,
            PassZone.Red => redColour,
            _ => Color.white // if anything else white
        };
    }
    private void ApplyColourToSprite(SpriteRenderer spriteRenderer, Color colour)
    {
        if (spriteRenderer != null) spriteRenderer.color = colour;
    }
    private void ApplyColourToMesh(MeshRenderer meshRenderer, Color colour)
    {
        if (meshRenderer != null) meshRenderer.material.color = colour;
    }
}