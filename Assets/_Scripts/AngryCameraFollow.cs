using UnityEngine;

public class AngryCameraFollow : MonoBehaviour
{
    private enum CameraState
    {
        WaitingAtStart,
        PanningToPlayer,
        Idle,
        Following,
        PanningLevel
    }

    [Header("Targets")]
    public Transform player;
    public Transform enemyFocusPoint;

    [Header("Movement Settings")]
    [Tooltip("How long the camera waits before the initial pan to the player.")]
    public float initialWaitTime = 1f;
    [Tooltip("How fast the camera follows the player.")]
    public float followSpeed = 8f;
    [Tooltip("How fast the player can drag the camera to pan. A value around 0.5-1 works well.")]
    public float panSpeed = 0.5f;
    [Tooltip("How long the initial pan from enemies to player takes.")]
    public float panToPlayerDuration = 2.0f;

    [Header("Camera Boundaries")]
    public float leftLimit = -10f;
    public float rightLimit = 30f;
    public float bottomLimit = -5f;
    public float topLimit = 15f;

    private CameraState currentState;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private float panTimer;

    void Start()
    {
        if (enemyFocusPoint != null)
        {
            transform.position = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, transform.position.z);
        }

        offset = new Vector3(0, 0, transform.position.z);
        currentState = CameraState.WaitingAtStart;
        panTimer = 0f;
    }

    void LateUpdate()
    {
        switch (currentState)
        {
            case CameraState.WaitingAtStart:
                HandleWaitingState();
                break;
            case CameraState.PanningToPlayer:
                HandleInitialPan();
                break;
            case CameraState.Idle:
                HandleIdleState();
                break;
            case CameraState.PanningLevel:
                HandlePanningState();
                break;
            case CameraState.Following:
                HandleFollowingState();
                break;
        }
    }

    void HandleWaitingState()
    {
        panTimer += Time.deltaTime;
        if (panTimer >= initialWaitTime)
        {
            panTimer = 0f;
            currentState = CameraState.PanningToPlayer;
        }
    }

    void HandleInitialPan()
    {
        panTimer += Time.deltaTime;
        float panRatio = panTimer / panToPlayerDuration;
        Vector3 startPos = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, offset.z);
        Vector3 endPos = new Vector3(player.position.x, player.position.y, offset.z);
        transform.position = Vector3.Lerp(startPos, endPos, panRatio);

        if (panRatio >= 1f)
        {
            currentState = CameraState.Idle;
        }
    }

    void HandleIdleState()
    {
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider == null)
            {
                lastMousePosition = Input.mousePosition;
                currentState = CameraState.PanningLevel;
            }
        }
    }

    void HandlePanningState()
    {
        if (Input.GetMouseButton(0))
        {
            // Calculate the difference in mouse position from the last frame
            Vector3 delta = lastMousePosition - Input.mousePosition;

            // Move the camera using that delta, scaled by our pan speed
            transform.Translate(delta * panSpeed * Time.deltaTime, Space.World);

            // Update the last mouse position for the next frame's calculation
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentState = CameraState.Idle;
        }

        // After every movement, clamp the camera's position to stay within the boundaries
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, leftLimit, rightLimit),
            Mathf.Clamp(transform.position.y, bottomLimit, topLimit),
            offset.z // Keep the original Z position
        );
    }

    void HandleFollowingState()
    {
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Also clamp the following camera
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, leftLimit, rightLimit),
            Mathf.Clamp(transform.position.y, bottomLimit, topLimit),
            offset.z
        );
    }

    public void StartFollowing()
    {
        currentState = CameraState.Following;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 topLeft = new Vector3(leftLimit, topLimit, 0);
        Vector3 topRight = new Vector3(rightLimit, topLimit, 0);
        Vector3 bottomLeft = new Vector3(leftLimit, bottomLimit, 0);
        Vector3 bottomRight = new Vector3(rightLimit, bottomLimit, 0);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}