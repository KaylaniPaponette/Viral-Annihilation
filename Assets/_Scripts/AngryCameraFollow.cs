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

    [Header("Zoom Settings")]
    [Tooltip("How sensitive the pinch-to-zoom is.")]
    public float zoomSpeed = 0.01f;
    [Tooltip("The smallest orthographic size (most zoomed in).")]
    public float minZoom = 4f;
    [Tooltip("The largest orthographic size (most zoomed out).")]
    public float maxZoom = 12f;

    private CameraState currentState;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private float panTimer;

    private float cameraHeight;
    private float cameraWidth;

    public static AngryCameraFollow Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
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

        HandleZoom();

        Camera mainCamera = Camera.main;
        cameraHeight = 2f * mainCamera.orthographicSize;
        cameraWidth = cameraHeight * mainCamera.aspect;

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
        // --- NEW NULL CHECK ---
        if (player == null || enemyFocusPoint == null) return;

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
        // --- NEW NULL CHECK ---
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            if (Player.IsBeingDragged)
            {
                return;
            }

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
    }

    void HandleFollowingState()
    {
        // --- THE MAIN FIX IS HERE ---
        // If the player has been destroyed, do nothing.
        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void StartFollowing()
    {
        currentState = CameraState.Following;
    }

    void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Camera mainCamera = Camera.main;
            mainCamera.orthographicSize -= difference * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
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
