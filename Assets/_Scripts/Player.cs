// ===== Player.cs =====
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    Vector3 startingPos;
    private Vector2 directiontoInitialPos;
    public float DirectionalInitialPosForce;
    private bool nukeThrown;
    float TimeSinceLaunch;
    public TextMeshProUGUI shotCountText; // UI Text reference
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
        startingPos = transform.position;
        //source = GetComponent<AudioSource>();
        UpdateShotCountUI();

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
    //private void Awake()
    //{
    //    startingPos = transform.position;
    //    source = GetComponent<AudioSource>();
    //    UpdateShotCountUI();

    //    // Subscribe to events
    //    if (GameManager.OnShotCountChanged != null)
    //    {
    //        GameManager.OnShotCountChanged += OnShotCountChanged;
    //    }
    //}

    //private void OnDestroy()
    //{
    //    // Unsubscribe from events
    //    if (GameManager.OnShotCountChanged != null)
    //    {
    //        GameManager.OnShotCountChanged -= OnShotCountChanged;
    //    }
    //}

    private void OnShotCountChanged(int newCount)
    {
        UpdateShotCountUI();
    }

    private void Update()
    {
        // Skip logic if we're already resetting
        if (isResetting) return;

        GetComponent<LineRenderer>().SetPosition(1, startingPos);
        GetComponent<LineRenderer>().SetPosition(0, transform.position);

        // Check if the projectile is out of bounds or has stopped moving
        if (transform.position.x <= -30 || transform.position.x >= 20
            || transform.position.y <= -20 || transform.position.y >= 20
            || TimeSinceLaunch >= 2f)
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
        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<LineRenderer>().enabled = true;
        //source.clip = TensionClip;
        //source.Play();
        // --- UPDATED CODE ---
        // Play the tension sound via the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(tensionSfxIndex);
        }
    }

    private void OnMouseUp()
    {
        nukeThrown = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        directiontoInitialPos = startingPos - transform.position;
        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<LineRenderer>().enabled = false;
        //source.clip = LaunchClip;
        //source.Play();
        // --- UPDATED CODE ---
        // Play the launch sound via the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(launchSfxIndex);
        }
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

    // Function to update shot count UI text
    private void UpdateShotCountUI()
    {
        if (shotCountText != null && GameManager.Instance != null)
        {
            int shotsLeft = GameManager.Instance.maxShots - GameManager.Instance.GetShotCount();
            shotCountText.text = "Shots Left: " + shotsLeft;
        }
    }
}


//-------------------------------------------------------
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;
//public class Player : MonoBehaviour
//{
//    Vector3 startingPos;
//    private Vector2 directiontoInitialPos;
//    public float DirectionalInitialPosForce;
//    private bool nukeThrown;
//    float TimeSinceLaunch;
//    public TextMeshProUGUI shotCountText; // UI Text reference
//    AudioSource source;
//    public AudioClip TensionClip;
//    public AudioClip LaunchClip;

//    // Game settings
//    public int maxShots = 3;
//    private int shotCount = 0;
//    public string gameOverSceneName = "_Scenes/GameOver";

//    // Flag to prevent multiple resets
//    private bool isResetting = false;

//    private void Awake()
//    {
//        startingPos = transform.position;
//        source = GetComponent<AudioSource>();

//        // If GameManager exists, use it for shot count
//        if (GameManager.Instance != null)
//        {
//            shotCount = GameManager.Instance.GetShotCount();
//            Debug.Log($"Player initialized with shot count from GameManager: {shotCount}");
//        }
//        else
//        {
//            // Otherwise use PlayerPrefs directly
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
//            Debug.Log($"Player initialized with shot count from PlayerPrefs: {shotCount}");
//        }

//        UpdateShotCountUI();
//    }

//    private void OnEnable()
//    {
//        // Subscribe to GameManager events
//        GameManager.OnShotCountChanged += OnShotCountChanged;
//    }

//    private void OnDisable()
//    {
//        // Unsubscribe from GameManager events
//        GameManager.OnShotCountChanged -= OnShotCountChanged;
//    }

//    private void OnShotCountChanged(int newCount)
//    {
//        Debug.Log($"Shot count changed event received: {newCount}");
//        UpdateShotCountUI();
//    }

//    private void Update()
//    {
//        // Skip logic if we're already resetting
//        if (isResetting)
//            return;

//        GetComponent<LineRenderer>().SetPosition(1, startingPos);
//        GetComponent<LineRenderer>().SetPosition(0, transform.position);

//        // Check if the projectile is out of bounds or has stopped moving
//        if (transform.position.x <= -30 || transform.position.x >= 20
//            || transform.position.y <= -20 || transform.position.y >= 20
//            || TimeSinceLaunch >= 2f)
//        {
//            // Mark that we're resetting to prevent multiple calls
//            isResetting = true;

//            // Increment shot count
//            IncrementShotCount();

//            // Reload current scene (unless we should go to game over)
//            if (shotCount < maxShots)
//            {
//                string currentLoadScene = SceneManager.GetActiveScene().name;
//                SceneManager.LoadScene(currentLoadScene);
//            }
//        }

//        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
//        {
//            TimeSinceLaunch += Time.deltaTime;
//        }
//    }

//    // Function to increment shot count with built-in Game Over check
//    private void IncrementShotCount()
//    {
//        shotCount++;
//        Debug.Log($"Shot count increased to {shotCount}/{maxShots}");

//        // Save to PlayerPrefs
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Update UI
//        UpdateShotCountUI();

//        // Notify GameManager if it exists
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.IncrementShotCount();
//        }

//        //// IMPORTANT: Direct Game Over check as a fallback
//        //if (shotCount >= maxShots)
//        //{
//        //    Debug.Log("Maximum shots reached - Going to Game Over scene directly");

//        //    // Reset shot count
//        //    shotCount = 0;
//        //    PlayerPrefs.SetInt("ShotCount", 0);
//        //    PlayerPrefs.Save();

//        //    // If GameManager exists, use it
//        //    if (GameManager.Instance != null)
//        //    {
//        //        GameManager.Instance.ForceGameOver();
//        //    }
//        //    else
//        //    {
//        //        // Direct load as fallback
//        //        Debug.Log("Loading Game Over scene directly from Player script");
//        //        SceneManager.LoadScene(gameOverSceneName);
//        //    }
//        //}
//        // Replace your GameOver loading code with this:
//        if (shotCount >= maxShots)
//        {
//            Debug.Log("Maximum shots reached - Loading GameOver scene directly");
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", 0);
//            PlayerPrefs.Save();

//            Debug.Log("ABOUT TO LOAD GAMEOVER");
//            SceneManager.LoadScene("_Scenes/GameOver");
//            Debug.Log("AFTER LOADING GAMEOVER"); // This won't print if scene loads
//        }
//    }

//    private void OnMouseDown()
//    {
//        GetComponent<SpriteRenderer>().color = Color.red;
//        GetComponent<LineRenderer>().enabled = true;
//        source.clip = TensionClip;
//        source.Play();
//    }

//    private void OnMouseUp()
//    {
//        nukeThrown = true;
//        GetComponent<SpriteRenderer>().color = Color.white;
//        directiontoInitialPos = startingPos - transform.position;
//        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
//        GetComponent<Rigidbody2D>().gravityScale = 1;
//        GetComponent<LineRenderer>().enabled = false;
//        source.clip = LaunchClip;
//        source.Play();
//    }

//    private void OnMouseDrag()
//    {
//        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
//    }

//    // Function to update shot count UI text
//    private void UpdateShotCountUI()
//    {
//        if (shotCountText != null)
//        {
//            int shotsLeft = maxShots - shotCount;
//            shotCountText.text = "Shots Left: " + shotsLeft;
//            Debug.Log($"Updated UI: Shots Left = {shotsLeft}");
//        }
//    }
//}

//--------------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//public class Player : MonoBehaviour
//{
//    Vector3 startingPos;
//    private Vector2 directiontoInitialPos;
//    public float DirectionalInitialPosForce;
//    private bool nukeThrown;
//    float TimeSinceLaunch;
//    public TextMeshProUGUI shotCountText; // UI Text reference
//    AudioSource source;
//    public AudioClip TensionClip;
//    public AudioClip LaunchClip;

//    // Flag to prevent multiple calls when resetting
//    private bool isResetting = false;

//    private void Awake()
//    {
//        startingPos = transform.position;
//        source = GetComponent<AudioSource>();
//        UpdateShotCountUI(); // Update UI at start

//        // Subscribe to shot count changed events
//        GameManager.OnShotCountChanged += OnShotCountChanged;
//    }

//    private void OnDestroy()
//    {
//        // Unsubscribe from events
//        GameManager.OnShotCountChanged -= OnShotCountChanged;
//    }

//    private void OnShotCountChanged(int newCount)
//    {
//        UpdateShotCountUI();
//    }

//    private void Update()
//    {
//        // Skip logic if we're already resetting
//        if (isResetting)
//            return;

//        GetComponent<LineRenderer>().SetPosition(1, startingPos);
//        GetComponent<LineRenderer>().SetPosition(0, transform.position);

//        // Check if the projectile is out of bounds or has stopped moving
//        if (transform.position.x <= -30 || transform.position.x >= 20
//            || transform.position.y <= -20 || transform.position.y >= 20
//            || TimeSinceLaunch >= 2f)
//        {
//            // Mark that we're resetting to prevent multiple calls
//            isResetting = true;

//            // Let the GameManager handle the shot count
//            if (GameManager.Instance != null)
//            {
//                GameManager.Instance.IncrementShotCount();
//                UpdateShotCountUI();
//            }

//            // Reload current scene
//            string currentLoadScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//            UnityEngine.SceneManagement.SceneManager.LoadScene(currentLoadScene);
//        }

//        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
//        {
//            TimeSinceLaunch += Time.deltaTime;
//        }
//    }

//    private void OnMouseDown()
//    {
//        GetComponent<SpriteRenderer>().color = Color.red;
//        GetComponent<LineRenderer>().enabled = true;
//        source.clip = TensionClip;
//        source.Play();
//    }

//    private void OnMouseUp()
//    {
//        nukeThrown = true;
//        GetComponent<SpriteRenderer>().color = Color.white;
//        directiontoInitialPos = startingPos - transform.position;
//        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
//        GetComponent<Rigidbody2D>().gravityScale = 1;
//        GetComponent<LineRenderer>().enabled = false;
//        source.clip = LaunchClip;
//        source.Play();
//    }

//    private void OnMouseDrag()
//    {
//        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
//    }

//    // Function to update shot count UI text
//    private void UpdateShotCountUI()
//    {
//        if (shotCountText != null && GameManager.Instance != null)
//        {
//            int shotsLeft = GameManager.Instance.maxShots - GameManager.Instance.GetShotCount();
//            shotCountText.text = "Shots Left: " + shotsLeft;
//        }
//    }
//}


//-------------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//public class Player : MonoBehaviour
//{
//    Vector3 startingPos;
//    private Vector2 directiontoInitialPos;
//    public float DirectionalInitialPosForce;
//    private bool nukeThrown;
//    float TimeSinceLaunch;
//    public TextMeshProUGUI shotCountText; // UI Text reference
//    AudioSource source;
//    public AudioClip TensionClip;
//    public AudioClip LaunchClip;

//    private void Awake()
//    {
//        startingPos = transform.position;
//        source = GetComponent<AudioSource>();
//        UpdateShotCountUI(); // Update UI at start

//        // Subscribe to shot count changed events
//        GameManager.OnShotCountChanged += OnShotCountChanged;
//    }

//    private void OnDestroy()
//    {
//        // Unsubscribe from events
//        GameManager.OnShotCountChanged -= OnShotCountChanged;
//    }

//    private void OnShotCountChanged(int newCount)
//    {
//        UpdateShotCountUI();
//    }

//    private void Update()
//    {
//        GetComponent<LineRenderer>().SetPosition(1, startingPos);
//        GetComponent<LineRenderer>().SetPosition(0, transform.position);

//        if (transform.position.x <= -30 || transform.position.x >= 20
//            || transform.position.y <= -20 || transform.position.y >= 20
//            || TimeSinceLaunch >= 2f)
//        {
//            // Let the GameManager handle the shot count
//            GameManager.Instance.IncrementShotCount();
//            UpdateShotCountUI();

//            // Reload current scene
//            string currentLoadScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//            UnityEngine.SceneManagement.SceneManager.LoadScene(currentLoadScene);
//        }

//        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
//        {
//            TimeSinceLaunch += Time.deltaTime;
//        }
//    }

//    private void OnMouseDown()
//    {
//        GetComponent<SpriteRenderer>().color = Color.red;
//        GetComponent<LineRenderer>().enabled = true;
//        source.clip = TensionClip;
//        source.Play();
//    }

//    private void OnMouseUp()
//    {
//        nukeThrown = true;
//        GetComponent<SpriteRenderer>().color = Color.white;
//        directiontoInitialPos = startingPos - transform.position;
//        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);
//        GetComponent<Rigidbody2D>().gravityScale = 1;
//        GetComponent<LineRenderer>().enabled = false;
//        source.clip = LaunchClip;
//        source.Play();
//    }

//    private void OnMouseDrag()
//    {
//        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
//    }

//    // Function to update shot count UI text
//    private void UpdateShotCountUI()
//    {
//        if (shotCountText != null && GameManager.Instance != null)
//        {
//            int shotsLeft = GameManager.Instance.maxShots - GameManager.Instance.GetShotCount();
//            shotCountText.text = "Shots Left: " + shotsLeft;
//        }
//    }
//}


/*
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{

    Vector3 startingPos;
    private Vector2 directiontoInitialPos;
    public float DirectionalInitialPosForce;
    private bool nukeThrown;
    float TimeSinceLaunch;
    private int ShotCount;

    public TextMeshProUGUI shotCountText; // UI Text reference

    public int maxShots = 3; // Max shots before Game Over


    AudioSource source;

    public AudioClip TensionClip;
    public AudioClip LaunchClip;

    private void Awake()
    {
        startingPos = transform.position;
        source = GetComponent<AudioSource>();

        ShotCount = PlayerPrefs.GetInt("ShotCount", 0);
        UpdateShotCountUI(); // Update UI at start
    }

    private void Update()
    {

        GetComponent<LineRenderer>().SetPosition(1, startingPos);
        GetComponent<LineRenderer>().SetPosition(0, transform.position);

        if (transform.position.x <= -30 || transform.position.x >= 20
            || transform.position.y <= -20 || transform.position.y >= 20
            || TimeSinceLaunch >= 2f)
        {
            ShotCount++;
            PlayerPrefs.SetInt("ShotCount", ShotCount);
            PlayerPrefs.Save();

            UpdateShotCountUI(); // Update UI at start

            string currentLoadScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentLoadScene);
        }

        if (ShotCount >= maxShots)
        {
            PlayerPrefs.SetInt("ShotCount", 0); // Reset the counter
            PlayerPrefs.Save();
            SceneManager.LoadScene("_Scenes/GameOver");
        }


        if (nukeThrown == true && GetComponent<Rigidbody2D>().linearVelocity.magnitude <= 0.1f)
        {
            TimeSinceLaunch += Time.deltaTime;
        }


    }



    private void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<LineRenderer>().enabled = true;
        source.clip = TensionClip;
        source.Play();

    }

    private void OnMouseUp()
    {
        nukeThrown = true;

        GetComponent<SpriteRenderer>().color = Color.white;
        directiontoInitialPos = startingPos - transform.position;
        GetComponent<Rigidbody2D>().AddForce(directiontoInitialPos * DirectionalInitialPosForce);

        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<LineRenderer>().enabled = false;

        source.clip = LaunchClip;
        source.Play();

    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

    // Function to update UI text
    private void UpdateShotCountUI()
    {
        if (shotCountText != null)
        {
            int shotsLeft = maxShots - ShotCount;
            shotCountText.text = "Shots Left: " + shotsLeft;
        }
    }
}

*/

//------------------------------------------------
//// Start is called once before the first execution of Update after the MonoBehaviour is created
//void Start()
//{

//}

//// Update is called once per frame
//void Update()
//{

//    if (Input.GetMouseButtonDown(0))
//    {
//        GetComponent<SpriteRenderer>().color = Color.blue;
//    }

//    if (Input.GetMouseButtonUp(0))
//    {
//        GetComponent<SpriteRenderer>().color = Color.white;
//    }

//    if (Input.GetMouseButtonDown(0))
//    {
//        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
//    }
//}