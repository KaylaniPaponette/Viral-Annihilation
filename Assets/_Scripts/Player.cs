using UnityEngine;

public class Player : MonoBehaviour
{
    public AngryCameraFollow mainCamera;
    private Collider2D _collider;

    Vector3 startingPos;
    private Vector2 directiontoInitialPos;
    public float DirectionalInitialPosForce;

    private bool nukeThrown;

    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
    public float maxDragDistance = 3f;

    [Tooltip("Time in seconds before the scene resets after the nuke stops moving.")]
    public float resetTimeAfterStop = 2f;

    float TimeSinceLaunch;

    [Header("Sound Effect Indexes")]
    public int tensionSfxIndex;
    public int launchSfxIndex;

    private bool isResetting = false;
    public static bool IsBeingDragged { get; private set; }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        startingPos = transform.position;
        IsBeingDragged = false;

        try
        {
            GameManager.OnShotCountChanged += OnShotCountChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
        }
    }

    private void OnShotCountChanged(int newCount)
    {
        // Future logic can go here
    }

    private void Update()
    {
        if (isResetting) return;

        GetComponent<LineRenderer>().SetPosition(1, startingPos);
        GetComponent<LineRenderer>().SetPosition(0, transform.position);

        bool isOutOfBounds = false;
        if (AngryCameraFollow.Instance != null)
        {
            isOutOfBounds = transform.position.x <= AngryCameraFollow.Instance.leftLimit ||
                            transform.position.x >= AngryCameraFollow.Instance.rightLimit ||
                            transform.position.y <= AngryCameraFollow.Instance.bottomLimit ||
                            transform.position.y >= AngryCameraFollow.Instance.topLimit;
        }

        if (nukeThrown && (isOutOfBounds || TimeSinceLaunch >= resetTimeAfterStop))
        {
            isResetting = true;

            if (GameManager.Instance != null)
            {
                Debug.Log("Shot used - informing GameManager");
                GameManager.Instance.IncrementShotCount();
            }

            //string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            //Debug.Log($"Reloading current scene: {currentScene}");
            //UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            Destroy(gameObject);
        }

        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
        {
            TimeSinceLaunch += Time.deltaTime;
        }
    }

    private void OnMouseDown()
    {
        if (nukeThrown) return;
        IsBeingDragged = true;

        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<LineRenderer>().enabled = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(tensionSfxIndex);
        }
    }
    
    private void OnMouseUp()
    {
        if (nukeThrown) return;
        IsBeingDragged = false;

        nukeThrown = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        directiontoInitialPos = startingPos - transform.position;
        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<LineRenderer>().enabled = false;
        if (mainCamera != null)
        {
            mainCamera.StartFollowing();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(launchSfxIndex);
        }
    }

    private void OnMouseDrag()
    {
        if (nukeThrown) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector3 direction = mousePosition - startingPos;

        if (direction.magnitude > maxDragDistance)
        {
            direction = direction.normalized * maxDragDistance;
        }

        transform.position = startingPos + direction;
    }
}

//// ===== Player.cs =====
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class Player : MonoBehaviour
//{

//    public AngryCameraFollow mainCamera;
//    private Collider2D _collider;

//    Vector3 startingPos;
//    private Vector2 directiontoInitialPos;
//    public float DirectionalInitialPosForce;

//    private bool nukeThrown;

//    [Tooltip("The maximum distance the player can drag the nuke from its start point.")]
//    public float maxDragDistance = 3f;

//    [Tooltip("Time in seconds before the scene resets after the nuke stops moving.")]
//    public float resetTimeAfterStop = 2f;

//    // --- NEW BOUNDARY VARIABLES ---
//    [Header("Boundaries")]
//    [Tooltip("The minimum X position before the level resets.")]
//    [SerializeField] private float minXBoundary = -30f;
//    [Tooltip("The maximum X position before the level resets.")]
//    [SerializeField] private float maxXBoundary = 20f;
//    [Tooltip("The minimum Y position before the level resets.")]
//    [SerializeField] private float minYBoundary = -20f;
//    [Tooltip("The maximum Y position before the level resets.")]
//    [SerializeField] private float maxYBoundary = 20f;

//    // This is used to determine when to reset the player
//    float TimeSinceLaunch;

//    //AudioSource source;
//    //public AudioClip TensionClip;
//    //public AudioClip LaunchClip;

//    // --- NEW SFX VARIABLES ---
//    [Header("Sound Effect Indexes")]
//    public int tensionSfxIndex;
//    public int launchSfxIndex;


//    // Flag to prevent multiple resets
//    private bool isResetting = false;


//    private void Awake()
//    {
//        _collider = GetComponent<Collider2D>();

//        startingPos = transform.position;
//        //source = GetComponent<AudioSource>();

//        // Subscribe to events with try/catch
//        try
//        {
//            GameManager.OnShotCountChanged += OnShotCountChanged;
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
//        }
//    }

//    private void OnShotCountChanged(int newCount)
//    {
//        // We can leave this empty or add other player-specific logic that needs to happen
//        // when the shot count changes.
//    }

//    private void Update()
//    {
//        // Skip logic if we're already resetting
//        if (isResetting) return;

//        GetComponent<LineRenderer>().SetPosition(1, startingPos);
//        GetComponent<LineRenderer>().SetPosition(0, transform.position);

//        // Check if the projectile is out of bounds or has stopped moving
//        if (nukeThrown && (transform.position.x <= minXBoundary || transform.position.x >= maxXBoundary
//                                || transform.position.y <= minYBoundary || transform.position.y >= maxYBoundary
//                                || TimeSinceLaunch >= resetTimeAfterStop))

//        {
//            // Mark that we're resetting to prevent multiple calls
//            isResetting = true;

//            // Increment shot count in GameManager
//            if (GameManager.Instance != null)
//            {
//                Debug.Log("Shot used - informing GameManager");
//                GameManager.Instance.IncrementShotCount();
//            }

//            // Reload current scene if not game over
//            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//            Debug.Log($"Reloading current scene: {currentScene}");
//            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
//        }

//        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
//        {
//            TimeSinceLaunch += Time.deltaTime;
//        }
//    }

//    private void OnMouseDown()
//    {
//        // If a nuke has already been thrown, do nothing
//        if (nukeThrown) return;

//        // Ensure the collider is set to trigger to prevent physics interactions while dragging
//        //if (_collider != null) _collider.enabled = false;

//        // Change color and enable line renderer
//        GetComponent<SpriteRenderer>().color = Color.red;
//        GetComponent<LineRenderer>().enabled = true;

//        // Play the tension sound via the SoundManager
//        if (SoundManager.Instance != null)
//        {
//            SoundManager.Instance.PlaySFX(tensionSfxIndex);
//        }
//    }

//    private void OnMouseUp()
//    {
//        // If a nuke has already been thrown, do nothing
//        if (nukeThrown) return;

//        // Reset the collider to not be a trigger
//        //if (_collider != null) _collider.enabled = true;

//        nukeThrown = true;
//        GetComponent<SpriteRenderer>().color = Color.white;
//        directiontoInitialPos = startingPos - transform.position;
//        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
//        GetComponent<Rigidbody2D>().gravityScale = 1;
//        GetComponent<LineRenderer>().enabled = false;
//        if (mainCamera != null)
//        {
//            mainCamera.StartFollowing();
//        }
//        // Play the launch sound via the SoundManager
//        if (SoundManager.Instance != null)
//        {
//            SoundManager.Instance.PlaySFX(launchSfxIndex);
//        }
//    }

//    private void OnMouseDrag()
//    {
//        if (nukeThrown) return;

//        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        mousePosition.z = 0;

//        // This calculates the distance and direction from the start point
//        Vector3 direction = mousePosition - startingPos;

//        // This is the key part: if the distance is too big...
//        if (direction.magnitude > maxDragDistance)
//        {
//            // ...it clamps the position to the maximum allowed distance.
//            direction = direction.normalized * maxDragDistance;
//        }

//        // This sets the final, constrained position
//        transform.position = startingPos + direction;
//    }

//    /// <summary>
//    /// Draws a yellow wireframe box in the Scene view to visualize the reset boundaries.
//    /// </summary>
//    private void OnDrawGizmos()
//    {
//        // Set the color for the Gizmo
//        Gizmos.color = Color.yellow;

//        // Calculate the center and size for the Gizmo box
//        float centerX = (minXBoundary + maxXBoundary) / 2f;
//        float centerY = (minYBoundary + maxYBoundary) / 2f;
//        Vector3 center = new Vector3(centerX, centerY, 0);

//        float sizeX = maxXBoundary - minXBoundary;
//        float sizeY = maxYBoundary - minYBoundary;
//        Vector3 size = new Vector3(sizeX, sizeY, 0);

//        // Draw the wireframe cube
//        Gizmos.DrawWireCube(center, size);
//    }
//}