// ===== Player.cs =====
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    // This is used to determine when to reset the player
    float TimeSinceLaunch;

    // The direct reference to the UI text has been removed. UIManager now handles this.
    // public TextMeshProUGUI shotCountText; // UI Text reference <<< THIS LINE IS GONE

    //AudioSource source;
    //public AudioClip TensionClip;
    //public AudioClip LaunchClip;

    // --- NEW SFX VARIABLES ---
    [Header("Sound Effect Indexes")]
    public int tensionSfxIndex;
    public int launchSfxIndex;


    // Flag to prevent multiple resets
    private bool isResetting = false;


    private void Awake()
    {
        _collider = GetComponent<Collider2D>();

        startingPos = transform.position;
        //source = GetComponent<AudioSource>();

        // The call to UpdateShotCountUI() is removed because UIManager handles UI updates now.
        // UpdateShotCountUI(); <<< THIS LINE IS GONE

        // Subscribe to events with try/catch
        try
        {
            GameManager.OnShotCountChanged += OnShotCountChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not subscribe to GameManager events: " + e.Message);
        }
    }

    // This method now only exists to potentially trigger other logic in the future.
    // Its original purpose was to update the UI, which is no longer needed here.
    private void OnShotCountChanged(int newCount)
    {
        // We can leave this empty or add other player-specific logic that needs to happen
        // when the shot count changes.
    }

    private void Update()
    {
        // Skip logic if we're already resetting
        if (isResetting) return;

        GetComponent<LineRenderer>().SetPosition(1, startingPos);
        GetComponent<LineRenderer>().SetPosition(0, transform.position);

        // Check if the projectile is out of bounds or has stopped moving
        // Note: The original condition was commented out, so we are using the new one
        //if (transform.position.x <= -30 || transform.position.x >= 20
        //  || transform.position.y <= -20 || transform.position.y >= 20
        //  || TimeSinceLaunch >= 2f)
        if (nukeThrown && (transform.position.x <= -30 || transform.position.x >= 20
                                || transform.position.y <= -20 || transform.position.y >= 20
                                || TimeSinceLaunch >= resetTimeAfterStop))

        {
            // Mark that we're resetting to prevent multiple calls
            isResetting = true;

            // Increment shot count in GameManager
            if (GameManager.Instance != null)
            {
                Debug.Log("Shot used - informing GameManager");
                GameManager.Instance.IncrementShotCount();
            }

            // Reload current scene if not game over
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"Reloading current scene: {currentScene}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }

        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
        {
            TimeSinceLaunch += Time.deltaTime;
        }
    }

    private void OnMouseDown()
    {
        // If a nuke has already been thrown, do nothing
        if (nukeThrown) return;

        // Ensure the collider is set to trigger to prevent physics interactions while dragging
        //if (_collider != null) _collider.enabled = false;

        // Change color and enable line renderer
        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<LineRenderer>().enabled = true;

        // Play the tension sound via the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(tensionSfxIndex);
        }
    }

    private void OnMouseUp()
    {
        // If a nuke has already been thrown, do nothing
        if (nukeThrown) return;

        // Reset the collider to not be a trigger
        //if (_collider != null) _collider.enabled = true;

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
        // Play the launch sound via the SoundManager
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

        // This calculates the distance and direction from the start point
        Vector3 direction = mousePosition - startingPos;

        // This is the key part: if the distance is too big...
        if (direction.magnitude > maxDragDistance)
        {
            // ...it clamps the position to the maximum allowed distance.
            direction = direction.normalized * maxDragDistance;
        }

        // This sets the final, constrained position
        transform.position = startingPos + direction;
    }

    // This function has been removed because the UIManager now handles all UI updates.
    // private void UpdateShotCountUI() { ... } <<< THIS FUNCTION IS GONE
}