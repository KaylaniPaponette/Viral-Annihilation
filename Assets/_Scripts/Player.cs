// ===== Player.cs (Final Version) ===== UPDATED FOR EXPLOSION & RESET =====
using UnityEngine;
using System.Collections; // Required for Coroutines

public class Player : MonoBehaviour
{
    public AngryCameraFollow mainCamera;
    private Collider2D _collider;

    public Vector3 startingPos { get; private set; }
    public Quaternion startingRotation { get; private set; }
    private Vector2 directiontoInitialPos;
    public float DirectionalInitialPosForce;
    private Vector3 lastDragPosition;
    private SpriteRenderer _spriteRenderer;

    private bool nukeThrown;
    public static bool IsBeingDragged { get; private set; }

    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
    public float maxDragDistance = 3f;

    [Header("Safe Release Settings")]
    [Tooltip("The distance from the start point required to actually launch the nuke.")]
    public float releaseThreshold = 0.5f;
    [Tooltip("Color of the nuke when it's in the safe zone (won't fire).")]
    public Color safeZoneColor = Color.gray;

    private bool isResetting = false;

    [Header("Explosion Settings")]
    [Tooltip("The time in seconds from launch until the nuke explodes.")]
    public float timeToExplode = 7f;
    [Tooltip("The visual effect prefab to instantiate on explosion.")]
    public GameObject explosionPrefab;
    [Tooltip("The duration to wait after the explosion before resetting the level.")]
    public float postExplosionDelay = 1.5f;

    [Header("Sound Effect Indexes")]
    public int tensionSfxIndex;
    public int launchSfxIndex;
    public int explosionSfxIndex;

    [Header("Trajectory")]
    [Tooltip("The LineRenderer component used to draw the projectile's path.")]
    public LineRenderer trajectoryLineRenderer;

    [Header("Trajectory Settings")]
    [Tooltip("The number of points to calculate for the trajectory line.")]
    [SerializeField] private int trajectoryPoints = 30;
    [Tooltip("The time interval between each calculated trajectory point.")]
    [SerializeField] private float timeBetweenPoints = 0.1f;
    [Tooltip("The gravity value to use for trajectory prediction. Should be a positive number.")]
    public float trajectoryGravity = 9.81f;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Store both position AND rotation at the very start
        startingPos = transform.position;
        startingRotation = transform.rotation;

        IsBeingDragged = false;
        GetComponent<LineRenderer>().enabled = false;

        if (trajectoryLineRenderer != null)
        {
            trajectoryLineRenderer.enabled = false;
        }

        try
        {
            GameManager.OnShotCountChanged += OnShotCountChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
        }
    }

    private void OnShotCountChanged(int newCount) { }

    private void Update()
    {
        if (isResetting) return;

        if (IsBeingDragged)
        {
            GetComponent<LineRenderer>().SetPosition(0, transform.position);
            GetComponent<LineRenderer>().SetPosition(1, startingPos);
        }

        if (nukeThrown)
        {
            HandleBoundaries();
        }
    }

    private void OnMouseDown()
    {
        if (nukeThrown) return;

        IsBeingDragged = true;
        lastDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _spriteRenderer.color = Color.red;
        GetComponent<LineRenderer>().enabled = true;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(tensionSfxIndex);

        if (trajectoryLineRenderer != null)
        {
            trajectoryLineRenderer.enabled = true;
        }
    }

    private void OnMouseUp()
    {
        if (nukeThrown) return;

        IsBeingDragged = false;
        float distance = Vector3.Distance(transform.position, startingPos);

        // --- CHECK THE THRESHOLD ---
        if (distance < releaseThreshold)
        {
            // Cancel the shot and reset visually
            ResetPlayer();
            Debug.Log("Shot canceled: Within safe release threshold.");
            return; // Exit the method early so we don't launch!
        }

        // --- ORIGINAL LAUNCH LOGIC ---
        nukeThrown = true;
        _spriteRenderer.color = Color.white;

        directiontoInitialPos = startingPos - transform.position;
        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
        GetComponent<Rigidbody2D>().gravityScale = 1;

        GetComponent<LineRenderer>().enabled = false;
        if (trajectoryLineRenderer != null) trajectoryLineRenderer.enabled = false;

        if (AngryCameraFollow.Instance != null) AngryCameraFollow.Instance.ResumeFollowingPlayer();
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(launchSfxIndex);

        if (GameManager.Instance != null) GameManager.Instance.StopLevelTimer();

        StartCoroutine(ExplosionRoutine());
    }

    private void OnMouseDrag()
    {
        // 1. Safety Check: If already thrown, do nothing
        if (nukeThrown) return;

        // 2. Handle Movement: Update position based on mouse delta and sensitivity
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 delta = currentMousePosition - lastDragPosition;
        transform.position += delta * (SettingsManager.DragSensitivity * 2f); //

        // 3. Clamping: Ensure the player doesn't drag beyond maxDragDistance
        Vector3 directionFromStart = transform.position - startingPos;
        if (directionFromStart.magnitude > maxDragDistance)
        {
            transform.position = startingPos + directionFromStart.normalized * maxDragDistance; //
        }
        lastDragPosition = currentMousePosition; //

        // 4. Safe Zone Logic: Determine if we should show the trajectory and change color
        float distance = Vector3.Distance(transform.position, startingPos);

        if (distance < releaseThreshold)
        {
            // Player is too close to start; treat as "Canceling"
            _spriteRenderer.color = safeZoneColor;

            if (trajectoryLineRenderer != null)
            {
                trajectoryLineRenderer.enabled = false;
            }
        }
        else
        {
            // Player is far enough; treat as "Aiming"
            _spriteRenderer.color = Color.red; // Visual cue for "Ready to fire"

            if (trajectoryLineRenderer != null)
            {
                trajectoryLineRenderer.enabled = true;
                // Only calculate trajectory when it's actually visible
                DrawTrajectory();
            }
        }
    }

    private void DrawTrajectory()
    {
        if (trajectoryLineRenderer == null) return;
        var rb = GetComponent<Rigidbody2D>();

        Vector2 launchVelocity = (startingPos - transform.position) * DirectionalInitialPosForce / rb.mass * Time.fixedDeltaTime;

        trajectoryLineRenderer.positionCount = trajectoryPoints;
        Vector3[] points = new Vector3[trajectoryPoints];
        Vector2 startPos = transform.position;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeBetweenPoints;
            Vector2 point = new Vector2(
                startPos.x + launchVelocity.x * t,
                startPos.y + launchVelocity.y * t - 0.5f * trajectoryGravity * t * t
            );
            points[i] = point;
        }

        trajectoryLineRenderer.SetPositions(points);
    }

    private void HandleBoundaries()
    {
        if (AngryCameraFollow.Instance == null) return;
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector3 currentPosition = transform.position;
        bool boundaryHit = false;

        if (currentPosition.x <= AngryCameraFollow.Instance.leftLimit)
        {
            currentPosition.x = AngryCameraFollow.Instance.leftLimit;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            boundaryHit = true;
        }
        else if (currentPosition.x >= AngryCameraFollow.Instance.rightLimit)
        {
            currentPosition.x = AngryCameraFollow.Instance.rightLimit;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            boundaryHit = true;
        }

        if (currentPosition.y <= AngryCameraFollow.Instance.bottomLimit)
        {
            currentPosition.y = AngryCameraFollow.Instance.bottomLimit;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            boundaryHit = true;
        }
        else if (currentPosition.y >= AngryCameraFollow.Instance.topLimit)
        {
            currentPosition.y = AngryCameraFollow.Instance.topLimit;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            boundaryHit = true;
        }

        if (boundaryHit)
        {
            transform.position = currentPosition;
            rb.angularVelocity = 0;
        }
    }

    private IEnumerator ExplosionRoutine()
    {
        // 1. Wait for the specified time
        yield return new WaitForSeconds(timeToExplode);

        // Prevent further updates while exploding
        isResetting = true;

        // 2. Trigger explosion effects
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(explosionSfxIndex);
        }

        // 3. Hide the player object and stop its movement
        _spriteRenderer.enabled = false;
        _collider.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.gravityScale = 0;

        // 4. Wait a moment so the player can see the explosion
        yield return new WaitForSeconds(postExplosionDelay);

        // 5. Tell the GameManager to use a shot, which will trigger the reset
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UseShot();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Resetting player directly as a fallback.");
            ResetPlayer();
        }
    }

    public void ResetPlayer()
    {
        // Stop any explosion coroutines that might still be running
        StopAllCoroutines();

        nukeThrown = false;
        isResetting = false;

        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;

        transform.position = startingPos;
        transform.rotation = startingRotation; // <--- ADD THIS LINE

        // RE-ENABLE the components that were hidden
        _spriteRenderer.enabled = true;
        _spriteRenderer.color = Color.white;
        _collider.enabled = true;

        GetComponent<LineRenderer>().enabled = false;

        Debug.Log("Player has been reset with original orientation.");
    }
}




//// ===== Player.cs (Updated) =====
//using UnityEngine;

//public class Player : MonoBehaviour
//{
//    public AngryCameraFollow mainCamera;
//    private Collider2D _collider;

//    public Vector3 startingPos { get; private set; }
//    private Vector2 directiontoInitialPos;
//    public float DirectionalInitialPosForce;
//    private Vector3 lastDragPosition;

//    private bool nukeThrown;
//    public static bool IsBeingDragged { get; private set; }

//    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
//    public float maxDragDistance = 3f;
//    [Tooltip("Time in seconds before the scene resets after the nuke stops moving.")]
//    public float resetTimeAfterStop = 1.2f;

//    // Tracks time since the nuke was thrown to determine when to reset.
//    float TimeSinceLaunch;
//    private bool isResetting = false;

//    [Header("Sound Effect Indexes")]
//    public int tensionSfxIndex;
//    public int launchSfxIndex;

//    [Header("Trajectory")]
//    [Tooltip("The LineRenderer component used to draw the projectile's path.")]
//    public LineRenderer trajectoryLineRenderer;

//    [Header("Trajectory Settings")]
//    [Tooltip("The number of points to calculate for the trajectory line.")]
//    [SerializeField] private int trajectoryPoints = 30;
//    [Tooltip("The time interval between each calculated trajectory point.")]
//    [SerializeField] private float timeBetweenPoints = 0.1f;
//    [Tooltip("The gravity value to use for trajectory prediction. Should be a positive number.")]
//    public float trajectoryGravity = 9.81f;

//    private void Awake()
//    {
//        _collider = GetComponent<Collider2D>();
//        startingPos = transform.position;
//        IsBeingDragged = false;
//        GetComponent<LineRenderer>().enabled = false;

//        // Ensure the trajectory line is hidden at the start
//        if (trajectoryLineRenderer != null)
//        {
//            trajectoryLineRenderer.enabled = false;
//        }

//        // Subscribes to GameManager events if the manager exists.
//        try
//        {
//            GameManager.OnShotCountChanged += OnShotCountChanged;
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
//        }
//    }

//    // This method is required to subscribe to the OnShotCountChanged event, but needs no logic for now.
//    private void OnShotCountChanged(int newCount) { }

//    private void Update()
//    {
//        if (isResetting) return;

//        // If dragging, draw the slingshot band from the start point to the nuke.
//        if (IsBeingDragged)
//        {
//            GetComponent<LineRenderer>().SetPosition(0, transform.position);
//            GetComponent<LineRenderer>().SetPosition(1, startingPos);
//        }

//        if (nukeThrown)
//        {
//            HandleBoundaries();
//        }

//        // If the nuke has been thrown and has nearly stopped moving, start the reset timer.
//        if (nukeThrown && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.5f)
//        {
//            TimeSinceLaunch += Time.deltaTime;
//        }

//        // If the reset timer has finished, use a shot and reset the level.
//        if (TimeSinceLaunch >= resetTimeAfterStop)
//        {
//            isResetting = true;
//            if (GameManager.Instance != null)
//            {
//                GameManager.Instance.UseShot();
//            }
//            else
//            {
//                // Fallback in case the GameManager doesn't exist in the scene.
//                Debug.LogWarning("GameManager not found! Resetting player directly as a fallback.");
//                ResetPlayer();
//            }
//        }
//    }

//    private void OnMouseDown()
//    {
//        if (nukeThrown) return;

//        IsBeingDragged = true;
//        lastDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        GetComponent<SpriteRenderer>().color = Color.red;
//        GetComponent<LineRenderer>().enabled = true;
//        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(tensionSfxIndex);

//        // Show the trajectory prediction line.
//        if (trajectoryLineRenderer != null)
//        {
//            trajectoryLineRenderer.enabled = true;
//        }
//    }

//    private void OnMouseUp()
//    {
//        if (nukeThrown) return;

//        IsBeingDragged = false;
//        nukeThrown = true;
//        GetComponent<SpriteRenderer>().color = Color.white;

//        // Calculate the launch direction and apply the main force.
//        directiontoInitialPos = startingPos - transform.position;
//        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
//        GetComponent<Rigidbody2D>().gravityScale = 1;

//        // Hide the slingshot band and trajectory line.
//        GetComponent<LineRenderer>().enabled = false;
//        if (trajectoryLineRenderer != null)
//        {
//            trajectoryLineRenderer.enabled = false;
//        }

//        if (AngryCameraFollow.Instance != null) AngryCameraFollow.Instance.ResumeFollowingPlayer();
//        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(launchSfxIndex);

//        // Let the GameManager know the shot has been taken to stop the timer.
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.StopLevelTimer();
//        }
//    }

//    private void OnMouseDrag()
//    {
//        if (nukeThrown) return;

//        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        Vector3 delta = currentMousePosition - lastDragPosition;
//        transform.position += delta * (SettingsManager.DragSensitivity * 2f);

//        // Limit how far the nuke can be pulled from its starting position.
//        Vector3 directionFromStart = transform.position - startingPos;
//        if (directionFromStart.magnitude > maxDragDistance)
//        {
//            transform.position = startingPos + directionFromStart.normalized * maxDragDistance;
//        }
//        lastDragPosition = currentMousePosition;

//        // Update the trajectory prediction line as the player drags.
//        DrawTrajectory();
//    }

//    // Calculates and draws the predicted path of the projectile.
//    private void DrawTrajectory()
//    {
//        if (trajectoryLineRenderer == null) return;
//        var rb = GetComponent<Rigidbody2D>();

//        // Calculate the initial velocity for the prediction based on the actual launch force and the physics timestep.
//        // This makes the prediction perfectly match the real launch.
//        Vector2 launchVelocity = (startingPos - transform.position) * DirectionalInitialPosForce / rb.mass * Time.fixedDeltaTime;

//        trajectoryLineRenderer.positionCount = trajectoryPoints;
//        Vector3[] points = new Vector3[trajectoryPoints];
//        Vector2 startPos = transform.position;

//        // Use kinematic equations to calculate each point in the trajectory arc.
//        for (int i = 0; i < trajectoryPoints; i++)
//        {
//            float t = i * timeBetweenPoints;
//            Vector2 point = new Vector2(
//                startPos.x + launchVelocity.x * t,
//                startPos.y + launchVelocity.y * t - 0.5f * trajectoryGravity * t * t
//            );
//            points[i] = point;
//        }

//        trajectoryLineRenderer.SetPositions(points);
//    }

//    // Handles player interaction with camera boundaries.
//    private void HandleBoundaries()
//    {
//        if (AngryCameraFollow.Instance == null) return;
//        var rb = GetComponent<Rigidbody2D>();
//        if (rb == null) return;

//        Vector3 currentPosition = transform.position;
//        bool boundaryHit = false;

//        // Check each boundary and stop movement if it's hit.
//        if (currentPosition.x <= AngryCameraFollow.Instance.leftLimit)
//        {
//            currentPosition.x = AngryCameraFollow.Instance.leftLimit;
//            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//            boundaryHit = true;
//        }
//        else if (currentPosition.x >= AngryCameraFollow.Instance.rightLimit)
//        {
//            currentPosition.x = AngryCameraFollow.Instance.rightLimit;
//            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
//            boundaryHit = true;
//        }

//        if (currentPosition.y <= AngryCameraFollow.Instance.bottomLimit)
//        {
//            currentPosition.y = AngryCameraFollow.Instance.bottomLimit;
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
//            boundaryHit = true;
//        }
//        else if (currentPosition.y >= AngryCameraFollow.Instance.topLimit)
//        {
//            currentPosition.y = AngryCameraFollow.Instance.topLimit;
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
//            boundaryHit = true;
//        }

//        if (boundaryHit)
//        {
//            transform.position = currentPosition;
//            rb.angularVelocity = 0;
//        }
//    }

//    // Resets the player's state and position for the next shot.
//    public void ResetPlayer()
//    {
//        nukeThrown = false;
//        isResetting = false;
//        TimeSinceLaunch = 0f;

//        var rb = GetComponent<Rigidbody2D>();
//        rb.gravityScale = 0;
//        rb.linearVelocity = Vector2.zero;
//        rb.angularVelocity = 0;

//        transform.position = startingPos;
//        GetComponent<SpriteRenderer>().color = Color.white;
//        GetComponent<LineRenderer>().enabled = false;

//        Debug.Log("Player has been reset.");
//    }
//}





////// ====== Player.cs (MAGIC NUMBER TRAJECTORY LINE t(-_-)t ========
////// ===== Player.cs (Updated) =====
////using UnityEngine;

////public class Player : MonoBehaviour
////{
////    public AngryCameraFollow mainCamera;
////    private Collider2D _collider;

////    public Vector3 startingPos { get; private set; }
////    private Vector2 directiontoInitialPos;
////    public float DirectionalInitialPosForce;
////    private Vector3 lastDragPosition;

////    private bool nukeThrown;

////    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
////    public float maxDragDistance = 3f;
////    [Tooltip("Time in seconds before the scene resets after the nuke stops moving.")]
////    public float resetTimeAfterStop = 1.2f;

////    float TimeSinceLaunch;

////    [Header("Sound Effect Indexes")]
////    public int tensionSfxIndex;
////    public int launchSfxIndex;

////    private bool isResetting = false;
////    public static bool IsBeingDragged { get; private set; }

////    // --- NEW TRAJECTORY VARIABLES ---
////    [Header("Trajectory")]
////    [Tooltip("The LineRenderer component used to draw the projectile's path.")]
////    public LineRenderer trajectoryLineRenderer;

////    [Header("Trajectory Settings")]
////    [Tooltip("A separate, smaller force value used ONLY for drawing the trajectory line accurately.")]
////    public float trajectoryForceMultiplier = 20f; // <--- ADD THIS LINE    [Tooltip("The number of points to calculate for the trajectory line.")]
////    [SerializeField] private int trajectoryPoints = 30;
////    [Tooltip("The time interval between each calculated trajectory point.")]
////    [SerializeField] private float timeBetweenPoints = 0.1f;
////    [Tooltip("The gravity value to use for trajectory prediction. Should be a positive number.")]
////    public float trajectoryGravity = 9.81f;
////    //[Tooltip("A multiplier to exaggerate the predicted arc for visual feedback.")]
////    //public float trajectoryGravityMultiplier = 1f;

////    private void Awake()
////    {
////        _collider = GetComponent<Collider2D>();
////        startingPos = transform.position;
////        IsBeingDragged = false;
////        GetComponent<LineRenderer>().enabled = false;
////        // --- NEW ---
////        // Ensure the trajectory line is hidden at the start
////        if (trajectoryLineRenderer != null)
////        {
////            trajectoryLineRenderer.enabled = false;
////        }
////        try
////        {
////            GameManager.OnShotCountChanged += OnShotCountChanged;
////        }
////        catch (System.Exception e)
////        {
////            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
////        }
////    }

////    private void OnShotCountChanged(int newCount) { }

////    private void Update()
////    {
////        if (isResetting) return;

////        if (IsBeingDragged)
////        {
////            GetComponent<LineRenderer>().SetPosition(0, transform.position);
////            GetComponent<LineRenderer>().SetPosition(1, startingPos);
////        }

////        if (nukeThrown)
////        {
////            HandleBoundaries();
////        }

////        if (nukeThrown && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.5f)
////        {
////            TimeSinceLaunch += Time.deltaTime;
////        }

////        if (TimeSinceLaunch >= resetTimeAfterStop)
////        {
////            isResetting = true;
////            if (GameManager.Instance != null)
////            {
////                GameManager.Instance.UseShot();
////            }
////            else
////            {
////                Debug.LogWarning("GameManager not found! Resetting player directly as a fallback.");
////                ResetPlayer();
////            }
////        }
////    }

////    private void OnMouseDown()
////    {
////        if (nukeThrown) return;
////        IsBeingDragged = true;
////        lastDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
////        GetComponent<SpriteRenderer>().color = Color.red;
////        GetComponent<LineRenderer>().enabled = true;
////        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(tensionSfxIndex);

////        // Show the trajectory line
////        if (trajectoryLineRenderer != null)
////        {
////            trajectoryLineRenderer.enabled = true;
////        }
////        //// --- NEW ---
////        //// Tell the GameManager to start the timer when we start dragging.
////        //if (GameManager.Instance != null)
////        //{
////        //    GameManager.Instance.StartLevelTimer();
////        //}
////    }

////    private void OnMouseUp()
////    {
////        if (nukeThrown) return;
////        IsBeingDragged = false;
////        nukeThrown = true;
////        GetComponent<SpriteRenderer>().color = Color.white;
////        directiontoInitialPos = startingPos - transform.position;
////        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
////        GetComponent<Rigidbody2D>().gravityScale = 1;
////        GetComponent<LineRenderer>().enabled = false;

////        // --- NEW ---
////        // Hide the trajectory line
////        if (trajectoryLineRenderer != null)
////        {
////            trajectoryLineRenderer.enabled = false;
////        }

////        if (AngryCameraFollow.Instance != null) AngryCameraFollow.Instance.ResumeFollowingPlayer();
////        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(launchSfxIndex);

////        // --- NEW ---
////        // Tell the GameManager to stop the timer when we release.
////        if (GameManager.Instance != null)
////        {
////            GameManager.Instance.StopLevelTimer();
////        }
////    }

////    private void OnMouseDrag()
////    {
////        if (nukeThrown) return;
////        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
////        Vector3 delta = currentMousePosition - lastDragPosition;
////        transform.position += delta * (SettingsManager.DragSensitivity * 2f);
////        Vector3 directionFromStart = transform.position - startingPos;
////        if (directionFromStart.magnitude > maxDragDistance)
////        {
////            transform.position = startingPos + directionFromStart.normalized * maxDragDistance;
////        }
////        lastDragPosition = currentMousePosition;

////        // --- NEW ---
////        // Update the trajectory line's path
////        DrawTrajectory();
////    }

////    /// Calculates and draws the predicted path of the projectile.
////    private void DrawTrajectory()
////    {
////        if (trajectoryLineRenderer == null) return;
////        var rb = GetComponent<Rigidbody2D>();
////        // --- THIS IS THE CRITICAL CHANGE ---
////        // We now use the NEW 'trajectoryForceMultiplier' for the visual prediction ONLY.
////        // The actual launch force remains separate and is used in OnMouseUp.
////        Vector2 launchVelocity = (startingPos - transform.position) * trajectoryForceMultiplier / rb.mass;
////        // --- END OF CHANGE ---

////        // Set up the line renderer
////        trajectoryLineRenderer.positionCount = trajectoryPoints;
////        Vector3[] points = new Vector3[trajectoryPoints];
////        Vector2 startPos = transform.position;

////        for (int i = 0; i < trajectoryPoints; i++)
////        {
////            float t = i * timeBetweenPoints;

////            // Calculate the X and Y positions separately
////            Vector2 point = new Vector2(
////                startPos.x + launchVelocity.x * t,
////                startPos.y + launchVelocity.y * t - 0.5f * trajectoryGravity * t * t
////            );

////            points[i] = point;
////        }

////        trajectoryLineRenderer.SetPositions(points);
////    }
////    private void HandleBoundaries()
////    {
////        if (AngryCameraFollow.Instance == null) return;
////        var rb = GetComponent<Rigidbody2D>();
////        if (rb == null) return;
////        Vector3 currentPosition = transform.position;
////        bool boundaryHit = false;
////        if (currentPosition.x <= AngryCameraFollow.Instance.leftLimit)
////        {
////            currentPosition.x = AngryCameraFollow.Instance.leftLimit;
////            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
////            boundaryHit = true;
////        }
////        else if (currentPosition.x >= AngryCameraFollow.Instance.rightLimit)
////        {
////            currentPosition.x = AngryCameraFollow.Instance.rightLimit;
////            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
////            boundaryHit = true;
////        }
////        if (currentPosition.y <= AngryCameraFollow.Instance.bottomLimit)
////        {
////            currentPosition.y = AngryCameraFollow.Instance.bottomLimit;
////            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
////            boundaryHit = true;
////        }
////        else if (currentPosition.y >= AngryCameraFollow.Instance.topLimit)
////        {
////            currentPosition.y = AngryCameraFollow.Instance.topLimit;
////            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
////            boundaryHit = true;
////        }
////        if (boundaryHit)
////        {
////            transform.position = currentPosition;
////            rb.angularVelocity = 0;
////        }
////    }

////    public void ResetPlayer()
////    {
////        nukeThrown = false;
////        isResetting = false;
////        TimeSinceLaunch = 0f;
////        var rb = GetComponent<Rigidbody2D>();
////        rb.gravityScale = 0;
////        rb.linearVelocity = Vector2.zero;
////        rb.angularVelocity = 0;
////        transform.position = startingPos;
////        GetComponent<SpriteRenderer>().color = Color.white;
////        GetComponent<LineRenderer>().enabled = false;
////        Debug.Log("Player has been reset.");
////    }
////}





////// ===== Player.cs =====
////using UnityEngine;
////using UnityEngine.UI;
////using TMPro;

////public class Player : MonoBehaviour
////{

////    public AngryCameraFollow mainCamera;
////    private Collider2D _collider;

////    Vector3 startingPos;
////    private Vector2 directiontoInitialPos;
////    public float DirectionalInitialPosForce;

////    private bool nukeThrown;

////    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
////    public float maxDragDistance = 3f;

////    [Tooltip("Time in seconds before the scene resets after the nuke stops moving.")]
////    public float resetTimeAfterStop = 2f;

////    // --- NEW BOUNDARY VARIABLES ---
////    [Header("Boundaries")]
////    [Tooltip("The minimum X position before the level resets.")]
////    [SerializeField] private float minXBoundary = -30f;
////    [Tooltip("The maximum X position before the level resets.")]
////    [SerializeField] private float maxXBoundary = 20f;
////    [Tooltip("The minimum Y position before the level resets.")]
////    [SerializeField] private float minYBoundary = -20f;
////    [Tooltip("The maximum Y position before the level resets.")]
////    [SerializeField] private float maxYBoundary = 20f;

////    // This is used to determine when to reset the player
////    float TimeSinceLaunch;

////    //AudioSource source;
////    //public AudioClip TensionClip;
////    //public AudioClip LaunchClip;

////    // --- NEW SFX VARIABLES ---
////    [Header("Sound Effect Indexes")]
////    public int tensionSfxIndex;
////    public int launchSfxIndex;


////    // Flag to prevent multiple resets
////    private bool isResetting = false;


////    private void Awake()
////    {
////        _collider = GetComponent<Collider2D>();

////        startingPos = transform.position;
////        //source = GetComponent<AudioSource>();

////        // Subscribe to events with try/catch
////        try
////        {
////            GameManager.OnShotCountChanged += OnShotCountChanged;
////        }
////        catch (System.Exception e)
////        {
////            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
////        }
////    }

////    private void OnShotCountChanged(int newCount)
////    {
////        // We can leave this empty or add other player-specific logic that needs to happen
////        // when the shot count changes.
////    }

////    private void Update()
////    {
////        // Skip logic if we're already resetting
////        if (isResetting) return;

////        GetComponent<LineRenderer>().SetPosition(1, startingPos);
////        GetComponent<LineRenderer>().SetPosition(0, transform.position);

////        // Check if the projectile is out of bounds or has stopped moving
////        if (nukeThrown && (transform.position.x <= minXBoundary || transform.position.x >= maxXBoundary
////                                || transform.position.y <= minYBoundary || transform.position.y >= maxYBoundary
////                                || TimeSinceLaunch >= resetTimeAfterStop))

////        {
////            // Mark that we're resetting to prevent multiple calls
////            isResetting = true;

////            // Increment shot count in GameManager
////            if (GameManager.Instance != null)
////            {
////                Debug.Log("Shot used - informing GameManager");
////                GameManager.Instance.IncrementShotCount();
////            }

////            // Reload current scene if not game over
////            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
////            Debug.Log($"Reloading current scene: {currentScene}");
////            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
////        }

////        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
////        {
////            TimeSinceLaunch += Time.deltaTime;
////        }
////    }

////    private void OnMouseDown()
////    {
////        // If a nuke has already been thrown, do nothing
////        if (nukeThrown) return;

////        // Ensure the collider is set to trigger to prevent physics interactions while dragging
////        //if (_collider != null) _collider.enabled = false;

////        // Change color and enable line renderer
////        GetComponent<SpriteRenderer>().color = Color.red;
////        GetComponent<LineRenderer>().enabled = true;

////        // Play the tension sound via the SoundManager
////        if (SoundManager.Instance != null)
////        {
////            SoundManager.Instance.PlaySFX(tensionSfxIndex);
////        }
////    }

////    private void OnMouseUp()
////    {
////        // If a nuke has already been thrown, do nothing
////        if (nukeThrown) return;

////        // Reset the collider to not be a trigger
////        //if (_collider != null) _collider.enabled = true;

////        nukeThrown = true;
////        GetComponent<SpriteRenderer>().color = Color.white;
////        directiontoInitialPos = startingPos - transform.position;
////        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
////        GetComponent<Rigidbody2D>().gravityScale = 1;
////        GetComponent<LineRenderer>().enabled = false;
////        if (mainCamera != null)
////        {
////            mainCamera.StartFollowing();
////        }
////        // Play the launch sound via the SoundManager
////        if (SoundManager.Instance != null)
////        {
////            SoundManager.Instance.PlaySFX(launchSfxIndex);
////        }
////    }

////    private void OnMouseDrag()
////    {
////        if (nukeThrown) return;

////        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
////        mousePosition.z = 0;

////        // This calculates the distance and direction from the start point
////        Vector3 direction = mousePosition - startingPos;

////        // This is the key part: if the distance is too big...
////        if (direction.magnitude > maxDragDistance)
////        {
////            // ...it clamps the position to the maximum allowed distance.
////            direction = direction.normalized * maxDragDistance;
////        }

////        // This sets the final, constrained position
////        transform.position = startingPos + direction;
////    }

////    /// <summary>
////    /// Draws a yellow wireframe box in the Scene view to visualize the reset boundaries.
////    /// </summary>
////    private void OnDrawGizmos()
////    {
////        // Set the color for the Gizmo
////        Gizmos.color = Color.yellow;

////        // Calculate the center and size for the Gizmo box
////        float centerX = (minXBoundary + maxXBoundary) / 2f;
////        float centerY = (minYBoundary + maxYBoundary) / 2f;
////        Vector3 center = new Vector3(centerX, centerY, 0);

////        float sizeX = maxXBoundary - minXBoundary;
////        float sizeY = maxYBoundary - minYBoundary;
////        Vector3 size = new Vector3(sizeX, sizeY, 0);

////        // Draw the wireframe cube
////        Gizmos.DrawWireCube(center, size);
////    }
////}