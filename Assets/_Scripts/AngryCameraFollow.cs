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
    [Tooltip("The horizontal offset from the player. A positive value keeps the player on the left of the screen.")]
    public float playerScreenOffsetX = 5f;

    [Header("Camera Boundaries")]
    [Tooltip("The leftmost point the camera's edge can reach.")]
    public float leftLimit = -10f;
    [Tooltip("The rightmost point the camera's edge can reach.")]
    public float rightLimit = 30f;
    [Tooltip("The bottommost point the camera's edge can reach.")]
    public float bottomLimit = -5f;
    [Tooltip("The topmost point the camera's edge can reach.")]
    public float topLimit = 15f;

    private CameraState currentState;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private float panTimer;

    // NEW VARIABLES for camera view size
    private float cameraHeight;
    private float cameraWidth;

    // --- NEW SINGLETON CODE ---
    public static AngryCameraFollow Instance { get; private set; }

    void Awake()
    {
        // This is the singleton pattern. It ensures there's only one instance.
        if (Instance != null && Instance != this)
        {
            // If another instance exists, destroy this one.
            Destroy(gameObject);
        }
        else
        {
            // Otherwise, set this as the one and only instance.
            Instance = this;
        }
    }
    void Start()
    {
        if (enemyFocusPoint != null)
        {
            transform.position = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, transform.position.z);
        }

        offset = new Vector3(0, 0, transform.position.z);
        currentState = CameraState.WaitingAtStart;
        panTimer = 0f;

        // NEW: Calculate camera's view size in world units
        Camera mainCamera = Camera.main;
        cameraHeight = 2f * mainCamera.orthographicSize;
        cameraWidth = cameraHeight * mainCamera.aspect;
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

        // NEW: Consolidated and corrected clamping logic
        float minX = leftLimit + cameraWidth / 2;
        float maxX = rightLimit - cameraWidth / 2;
        float minY = bottomLimit + cameraHeight / 2;
        float maxY = topLimit - cameraHeight / 2;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
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
        Vector3 endPos = new Vector3(player.position.x + playerScreenOffsetX, player.position.y, offset.z);
        transform.position = Vector3.Lerp(startPos, endPos, panRatio);

        if (panRatio >= 1f)
        {
            currentState = CameraState.Idle;
        }
    }

    void HandleIdleState()
    {
        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            // NEW CHECK: If the player is being dragged, do nothing.
            if (Player.IsBeingDragged)
            {
                return;
            }

            // The original logic now only runs if the player is NOT being dragged
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
            Vector3 delta = lastMousePosition - Input.mousePosition;
            transform.Translate(delta * panSpeed * Time.deltaTime, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentState = CameraState.Idle;
        }
        // REMOVED clamping from here
    }

    void HandleFollowingState()
    {
        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // REMOVED clamping from here
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