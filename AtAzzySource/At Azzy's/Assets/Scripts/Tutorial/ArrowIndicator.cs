/*
 * Displays a directional UI arrow that guides the player toward
 * a world-space objective.
 *
 * Features:
 * - Tracks both on-screen and off-screen targets
 * - Smooth rotational movement
 * - Screen-edge clamping
 * - Automatic visibility based on distance
 */

using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float edgePadding = 60f;
    [SerializeField] private float rotationOffset = 90f;

    private Transform target;
    private Canvas canvas;

    /*
     * Initializes component references and validates required UI elements.
     * Automatically assigns the main camera if none is provided.
     * Disables the component if the arrow UI reference is missing.
     */
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        canvas = GetComponentInParent<Canvas>();

        if (arrowUI == null)
        {
            Debug.LogError("ArrowIndicator: arrowUI is NOT assigned!", this);
            enabled = false;
            return;
        }

        arrowUI.gameObject.SetActive(false);
    }

    /*
     * Assigns a world-space target for the arrow to track.
     * The arrow becomes visible automatically when a valid target is set.
     *
     * @param newTarget Transform of the objective to follow.
     */
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        arrowUI.gameObject.SetActive(target != null);
    }

    /*
     * Clears the current target and hides the arrow.
     * Typically used when the objective is completed or no longer relevant.
     */
    public void ClearTarget()
    {
        target = null;
        arrowUI.gameObject.SetActive(false);
    }

    /*
     * Updates the arrow every frame while a valid target exists.
     * Calculates direction and updates both position and rotation.
     */
    private void Update()
    {
        if (target == null || mainCamera == null)
            return;

        UpdateArrowTowardsTarget(target.position);
    }

    /*
     * Positions and rotates the arrow so it consistently points toward
     * the target in world space.
     *
     * Behavior:
     * - Targets behind the camera are flipped to maintain correct direction.
     * - The arrow is hidden when the target is within a defined radius
     *   near the center of the screen.
     * - Otherwise, the arrow is clamped to a circular boundary
     *   to ensure accurate directional guidance.
     *
     * @param targetWorldPos World position of the tracked target.
     */
    private void UpdateArrowTowardsTarget(Vector3 targetWorldPos)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
        bool isBehind = screenPos.z < 0f;

        if (isBehind)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        Vector2 screenCenter =
            new Vector2(Screen.width / 2f, Screen.height / 2f);

        Vector2 offset =
            (Vector2)screenPos - screenCenter;

        //----------------------------------
        // Hide arrow when the target is close to the screen center
        //----------------------------------

        float radius =
            Mathf.Min(Screen.width, Screen.height) / 2f - edgePadding;

        float hideRadius = radius * 1f; // Adjustable

        if (!isBehind && offset.sqrMagnitude < hideRadius * hideRadius)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);

        //----------------------------------
        // Keep arrow perfectly aligned with the target direction
        //----------------------------------

        Vector2 dir = offset.normalized;

        Vector2 edgePos =
            screenCenter + dir * radius;

        arrowUI.anchoredPosition =
            edgePos - screenCenter;

        //----------------------------------
        // Apply smooth rotation
        //----------------------------------

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion targetRot =
            Quaternion.Euler(0, 0, angle + rotationOffset);

        arrowUI.rotation =
            Quaternion.Lerp(
                arrowUI.rotation,
                targetRot,
                15f * Time.deltaTime
            );
    }
}
