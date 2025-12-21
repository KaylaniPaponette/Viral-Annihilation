// ===== AngryCameraFollow.cs (Final Version) =====
using UnityEngine;

public class AngryCameraFollow : MonoBehaviour
{
    private enum CameraState
    {
        WaitingAtStart,
        PanningToPlayer,
        Idle,
        Following,
        PanningLevel,
        ManualPanIdle
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
    public float leftLimit = -10f;
    public float rightLimit = 30f;
    public float bottomLimit = -5f;
    public float topLimit = 15f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.01f;
    public float minZoom = 4f;
    public float maxZoom = 12f;

    private CameraState currentState;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private float panTimer;
    private Vector3 lastPlayerPosition;
    private float cameraHeight;
    private float cameraWidth;
    private Camera _cam;
    private float _absoluteMaxZoom; // The zoom limit based on level boundaries

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

    //void Start()
    //{
    //    if (enemyFocusPoint != null)
    //    {
    //        transform.position = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, transform.position.z);
    //    }

    //    offset = new Vector3(0, 0, transform.position.z);
    //    currentState = CameraState.WaitingAtStart;
    //    panTimer = 0f;

    //    Camera mainCamera = Camera.main;
    //    cameraHeight = 2f * mainCamera.orthographicSize;
    //    cameraWidth = cameraHeight * mainCamera.aspect;
    //    if (player != null)
    //    {
    //        lastPlayerPosition = player.position;
    //    }
    //}
    void Start()
    {
        _cam = GetComponent<Camera>(); // Cache the component for performance

        if (enemyFocusPoint != null)
        {
            transform.position = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, transform.position.z);
        }

        offset = new Vector3(0, 0, transform.position.z);
        currentState = CameraState.WaitingAtStart;
        panTimer = 0f;

        // --- NEW: Calculate Absolute Max Zoom based on boundaries ---
        float levelWidth = rightLimit - leftLimit;
        float levelHeight = topLimit - bottomLimit;

        // We find the max zoom for height, and the max zoom for width (considering aspect ratio)
        float maxZoomByHeight = levelHeight / 2f;
        float maxZoomByWidth = (levelWidth / _cam.aspect) / 2f;

        // The absolute limit is whichever one is smaller (the first one to hit a wall)
        _absoluteMaxZoom = Mathf.Min(maxZoomByHeight, maxZoomByWidth);

        // If your manual 'maxZoom' is smaller than this, keep the manual one.
        // This prevents zooming out too far even in massive levels.
        maxZoom = Mathf.Min(maxZoom, _absoluteMaxZoom);
        // -------------------------------------------------------------

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
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
            case CameraState.ManualPanIdle:
                HandleManualPanIdle();
                break;
        }

        HandleZoom();
        ClampCameraPosition();
    }

    //// Extracted the clamping logic into its own method for clarity
    //void ClampCameraPosition()
    //{
    //    Camera mainCamera = Camera.main;
    //    cameraHeight = 2f * mainCamera.orthographicSize;
    //    cameraWidth = cameraHeight * mainCamera.aspect;

    //    float minX = leftLimit + cameraWidth / 2;
    //    float maxX = rightLimit - cameraWidth / 2;
    //    float minY = bottomLimit + cameraHeight / 2;
    //    float maxY = topLimit - cameraHeight / 2;

    //    Vector3 clampedPosition = transform.position;
    //    clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
    //    clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
    //    transform.position = clampedPosition;
    //}

    void ClampCameraPosition()
    {
        if (_cam == null) return;

        float currentHeight = 2f * _cam.orthographicSize;
        float currentWidth = currentHeight * _cam.aspect;

        float minX = leftLimit + currentWidth / 2f;
        float maxX = rightLimit - currentWidth / 2f;
        float minY = bottomLimit + currentHeight / 2f;
        float maxY = topLimit - currentHeight / 2f;

        Vector3 clampedPosition = transform.position;

        // Centering logic if camera is larger than bounds
        if (minX > maxX) clampedPosition.x = (leftLimit + rightLimit) / 2f;
        else clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);

        if (minY > maxY) clampedPosition.y = (bottomLimit + topLimit) / 2f;
        else clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

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
        if (player == null || enemyFocusPoint == null) return;

        panTimer += Time.deltaTime;
        float panRatio = panTimer / panToPlayerDuration;
        Vector3 startPos = new Vector3(enemyFocusPoint.position.x, enemyFocusPoint.position.y, transform.position.z);
        Vector3 endPos = new Vector3(player.position.x + playerScreenOffsetX, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(startPos, endPos, panRatio);

        if (panRatio >= 1f)
        {
            currentState = CameraState.Idle;
        }
    }

    void HandleIdleState()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        lastPlayerPosition = player.position;

        if (Input.GetMouseButtonDown(0) && !Player.IsBeingDragged)
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
            Vector3 delta = lastMousePosition - Input.mousePosition;
            transform.Translate(delta * panSpeed * Time.deltaTime, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentState = CameraState.ManualPanIdle;
        }
    }

    void HandleManualPanIdle()
    {
        if (Input.GetMouseButtonDown(0) && !Player.IsBeingDragged)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider == null)
            {
                lastMousePosition = Input.mousePosition;
                currentState = CameraState.PanningLevel;
            }
        }
    }

    public void ResumeFollowingPlayer()
    {
        currentState = CameraState.Following;
    }

    /// <summary>
    /// This is called by the Player script when the nuke is thrown.
    /// </summary>
    public void StartFollowing()
    {
        currentState = CameraState.Following;
    }

    void HandleFollowingState()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;
        targetPosition.x += playerScreenOffsetX;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        lastPlayerPosition = player.position;
    }

    /// <summary>
    /// Resets the camera to its initial state, focused on the player's start position.
    /// Called by the GameManager.
    /// </summary>
    public void ResetToStartPosition()
    {
        currentState = CameraState.Idle;
        // The player script has already been reset at this point, so 'player.position'
        // is now the player's starting position.
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            targetPosition.x += playerScreenOffsetX;
            // Move instantly, no Lerp needed for a hard reset.
            transform.position = targetPosition;
        }
        Debug.Log("Camera has been reset.");
    }

    //void HandleZoom()
    //{
    //    if (Input.touchCount == 2)
    //    {
    //        Touch touchZero = Input.GetTouch(0);
    //        Touch touchOne = Input.GetTouch(1);
    //        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
    //        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
    //        float difference = currentMagnitude - prevMagnitude;
    //        Camera mainCamera = Camera.main;
    //        mainCamera.orthographicSize -= difference * zoomSpeed;
    //        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
    //    }
    //}

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

        // Use the cached _cam instead of Camera.main
        _cam.orthographicSize -= difference * zoomSpeed;

        // Clamp using our dynamically calculated maxZoom
        _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, minZoom, maxZoom);
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
