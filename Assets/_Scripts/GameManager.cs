using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Shot tracking
    [SerializeField] private int shotCount = 0;
    public int maxShots = 3;

    [Header("Music Settings")] // Optional: Add a header for organization
    public int defaultBgmIndex = 0; // <<< ADD THIS LINE

    // Game state
    private bool isTransitioningToGameOver = false;
    private bool isTransitioningToNextLevel = false;
    
    // Level management
    [System.Serializable]
    public class LevelData
    {
        public string sceneName;
        public string nextLevelName;
        public int bgmIndex;
    }
    
    public List<LevelData> levelSequence = new List<LevelData>();
    private string currentLevel;
    
    // Enemy tag
    public string enemyTag = "Enemy";
    
    // Debug settings
    public bool debugMode = true;
    
    // Scene paths
    public string gameOverScenePath = "_Scenes/GameOver";
    
    // Events
    public delegate void ShotCountChanged(int newCount);
    public static event ShotCountChanged OnShotCountChanged;
    
    // Enemy check timer
    private float enemyCheckInterval = 0.5f;
    private float enemyCheckTimer = 0;
    
    // Initialize the singleton
    void Awake()
    {
        // Create singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized");
            
            // Load shot count
            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
            Debug.Log($"Starting with shot count: {shotCount}");
            
            // Listen for scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Print level sequence for debugging
            PrintLevelSequence();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void PrintLevelSequence()
    {
        Debug.Log("===== LEVEL SEQUENCE CONFIGURATION =====");
        if (levelSequence.Count == 0)
        {
            Debug.LogWarning("NO LEVELS CONFIGURED IN LEVEL SEQUENCE! Please add levels in the inspector.");
        }
        
        foreach (LevelData level in levelSequence)
        {
            Debug.Log($"Level: {level.sceneName} -> Next: {level.nextLevelName}");
        }
        Debug.Log("=======================================");
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        currentLevel = scene.name;
        
        // Check if the current scene has a path prefix
        string scenePath = scene.path;
        string sceneName = scene.name;
        Debug.Log($"Full scene path: {scenePath}, Scene name: {sceneName}");

        // --- START OF NEW CODE ---

        // Find the level data for the current scene
        LevelData currentLevelData = GetLevelData(currentLevel);

        // If we found data for this level and the SoundManager exists...
        if (currentLevelData != null && SoundManager.Instance != null)
        {
            // ...tell the SoundManager to play the BGM for this level.
            SoundManager.Instance.PlayBGM(currentLevelData.bgmIndex);
        }
        else if (SoundManager.Instance != null)
        {
            // Optional: If this scene is not in our level list (like a main menu), stop the music.
            // You could also play a default track here, e.g., SoundManager.Instance.PlayBGM(0);
            SoundManager.Instance.PlayBGM(defaultBgmIndex);
        }

        // --- END OF NEW CODE ---

        // If this is the GameOver scene, reset game state
        if (sceneName == "GameOver" || scenePath.Contains("/GameOver"))
        {
            Debug.Log("GameOver scene detected - resetting shot count");
            shotCount = 0;
            PlayerPrefs.SetInt("ShotCount", 0);
            PlayerPrefs.Save();
            isTransitioningToGameOver = false;
            isTransitioningToNextLevel = false;
        }
        else
        {
            // Not a game over scene, check if it's a gameplay level
            bool isGameplayLevel = IsLevelInSequence(sceneName);
            
            if (isGameplayLevel)
            {
                Debug.Log($"Gameplay level detected: {sceneName} - starting enemy check");
                // Reset transitioning flags
                isTransitioningToNextLevel = false;
                // Start enemy check after a delay to allow scene to fully load
                enemyCheckTimer = 0;
                
                // Check for enemies right away to establish baseline
                Invoke("InitialEnemyCheck", 0.2f);
            }
            else
            {
                Debug.Log($"Scene {sceneName} is not in level sequence - not checking for enemies");
            }
        }
    }

    // --- ADD THIS NEW HELPER FUNCTION ---
    // This function finds the LevelData that matches the given scene name.
    LevelData GetLevelData(string sceneName)
    {
        foreach (LevelData level in levelSequence)
        {
            if (level.sceneName == sceneName || level.sceneName.EndsWith("/" + sceneName))
            {
                return level;
            }
        }
        return null; // Return null if no match is found
    }

    void InitialEnemyCheck()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Debug.Log($"Initial enemy check for {currentLevel}: Found {enemies.Length} enemies with tag '{enemyTag}'");
        
        // If not seeing any enemies, check if tag is correct
        if (enemies.Length == 0)
        {
            Debug.LogWarning($"No enemies found with tag '{enemyTag}'. Make sure your enemies have this tag.");
            // List all available tags in the scene
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            HashSet<string> allTags = new HashSet<string>();
            foreach (GameObject obj in allObjects)
            {
                if (!string.IsNullOrEmpty(obj.tag) && obj.tag != "Untagged")
                {
                    allTags.Add(obj.tag);
                }
            }
            
            Debug.Log("Available tags in scene: " + string.Join(", ", allTags));
        }
    }
    
    bool IsLevelInSequence(string sceneName)
    {
        foreach (LevelData level in levelSequence)
        {
            // Check for both exact match and path suffix match
            if (level.sceneName == sceneName || 
                level.sceneName.EndsWith("/" + sceneName) ||
                sceneName.EndsWith("/" + level.sceneName))
            {
                return true;
            }
        }
        return false;
    }
    
    void Update()
    {
        // Only check for enemies in gameplay levels
        if (isTransitioningToGameOver || isTransitioningToNextLevel)
            return;
            
        // Check if the current level is in our sequence
        bool isGameplayLevel = IsLevelInSequence(currentLevel);
        
        if (isGameplayLevel)
        {
            // Update enemy check timer
            enemyCheckTimer += Time.deltaTime;
            
            // Check for enemies at intervals
            if (enemyCheckTimer >= enemyCheckInterval)
            {
                enemyCheckTimer = 0;
                CheckForEnemies();
            }
        }
    }
    
    void CheckForEnemies()
    {
        // Find all objects with the enemy tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        
        // Only log every few seconds to avoid spam
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Enemy check: Found {enemies.Length} enemies with tag '{enemyTag}'");
        }
        
        // If no enemies left, complete level
        if (enemies.Length == 0)
        {
            Debug.Log("All enemies destroyed - completing level");
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        // Prevent multiple calls
        if (isTransitioningToNextLevel) 
        {
            Debug.Log("Already transitioning to next level - ignoring duplicate call");
            return;
        }
        
        isTransitioningToNextLevel = true;
        Debug.Log($"Completing level: {currentLevel}");
        
        // Reset shot count for the next level
        shotCount = 0;
        PlayerPrefs.SetInt("ShotCount", 0);
        PlayerPrefs.Save();
        
        // Notify listeners
        if (OnShotCountChanged != null)
        {
            OnShotCountChanged(shotCount);
        }
        
        // Find next level
        string nextLevel = GetNextLevelName(currentLevel);
        
        if (!string.IsNullOrEmpty(nextLevel))
        {
            Debug.Log($"Going to next level: {nextLevel}");
            // Give a slight delay before loading next level
            Invoke("LoadNextLevel", 0.5f);
        }
        else
        {
            Debug.LogWarning($"No next level found for {currentLevel} - check your level sequence configuration!");
        }
    }
    
    void LoadNextLevel()
    {
        string nextLevel = GetNextLevelName(currentLevel);
        if (!string.IsNullOrEmpty(nextLevel))
        {
            Debug.Log($"Loading next level: {nextLevel}");
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            Debug.LogError("Tried to load next level but nextLevel is empty!");
        }
    }
    
    string GetNextLevelName(string currentLevelName)
    {
        Debug.Log($"Looking for next level after: {currentLevelName}");
        
        foreach (LevelData level in levelSequence)
        {
            // Compare with exact name and path variations
            if (level.sceneName == currentLevelName || 
                level.sceneName.EndsWith("/" + currentLevelName) ||
                currentLevelName.EndsWith("/" + level.sceneName))
            {
                Debug.Log($"Found match! Next level: {level.nextLevelName}");
                return level.nextLevelName;
            }
        }
        
        Debug.LogWarning($"No match found for {currentLevelName} in level sequence!");
        return "";
    }
    
    // Called by Player when shooting
    public void IncrementShotCount()
    {
        // If we're already going to GameOver or next level, don't process more shots
        if (isTransitioningToGameOver || isTransitioningToNextLevel) 
        {
            Debug.Log("Already transitioning - ignoring shot");
            return;
        }
        
        shotCount++;
        Debug.Log($"Shot count increased to {shotCount}/{maxShots}");
        
        // Save to PlayerPrefs
        PlayerPrefs.SetInt("ShotCount", shotCount);
        PlayerPrefs.Save();
        
        // Notify listeners
        if (OnShotCountChanged != null)
        {
            OnShotCountChanged(shotCount);
        }
        
        // Check for GameOver
        if (shotCount >= maxShots)
        {
            Debug.Log("GAME OVER - Max shots reached");
            GoToGameOver();
        }
    }
    
    // Called to go to the GameOver scene
    public void GoToGameOver()
    {
        // Prevent multiple calls
        if (isTransitioningToGameOver) return;
        
        isTransitioningToGameOver = true;
        Debug.Log("Going to GameOver scene");
        
        // Add a frame delay to avoid conflicts
        Invoke("LoadGameOverScene", 0.1f);
    }
    
    // Actual scene loading function with delay
    private void LoadGameOverScene()
    {
        Debug.Log("Loading GameOver scene now");
        SceneManager.LoadScene(gameOverScenePath);
    }
    
    // Get current shot count
    public int GetShotCount()
    {
        return shotCount;
    }
    
    // Clean up event listeners
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // This method can be called from editor to test enemy detection
    [ContextMenu("Force Check For Enemies")]
    public void ForceCheckForEnemies()
    {
        InitialEnemyCheck();
    }
    
    // This method can be called from editor to test level completion
    [ContextMenu("Force Complete Level")]
    public void ForceCompleteLevel()
    {
        CompleteLevel();
    }
}


//// ===== GameManager.cs =====
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;
//using TMPro;

//public class GameManager : MonoBehaviour
//{
//    // Singleton instance
//    public static GameManager Instance { get; private set; }

//    // Shot tracking
//    [SerializeField] private int shotCount = 0;
//    public int maxShots = 3;

//    // Game state
//    private bool isTransitioningToGameOver = false;

//    // Scene paths
//    public string gameOverScenePath = "_Scenes/GameOver";

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    // Initialize the singleton
//    void Awake()
//    {
//        // Create singleton
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//            Debug.Log("GameManager initialized");

//            // Load shot count
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
//            Debug.Log($"Starting with shot count: {shotCount}");

//            // Listen for scene changes
//            SceneManager.sceneLoaded += OnSceneLoaded;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        Debug.Log($"Scene loaded: {scene.name}");

//        // If this is the GameOver scene, reset game state
//        if (scene.name == "GameOver" || scene.name.EndsWith("/GameOver"))
//        {
//            Debug.Log("GameOver scene detected - resetting shot count");
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", 0);
//            PlayerPrefs.Save();
//            isTransitioningToGameOver = false;
//        }
//    }

//    // Called by Player when shooting
//    public void IncrementShotCount()
//    {
//        // If we're already going to GameOver, don't process more shots
//        if (isTransitioningToGameOver)
//        {
//            Debug.Log("Already transitioning to GameOver - ignoring shot");
//            return;
//        }

//        shotCount++;
//        Debug.Log($"Shot count increased to {shotCount}/{maxShots}");

//        // Save to PlayerPrefs
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//        {
//            OnShotCountChanged(shotCount);
//        }

//        // Check for GameOver
//        if (shotCount >= maxShots)
//        {
//            Debug.Log("GAME OVER - Max shots reached");
//            GoToGameOver();
//        }
//    }

//    // Called to go to the GameOver scene
//    public void GoToGameOver()
//    {
//        // Prevent multiple calls
//        if (isTransitioningToGameOver) return;

//        isTransitioningToGameOver = true;
//        Debug.Log("Going to GameOver scene");

//        // Add a frame delay to avoid conflicts
//        Invoke("LoadGameOverScene", 0.1f);
//    }

//    // Actual scene loading function with delay
//    private void LoadGameOverScene()
//    {
//        Debug.Log("Loading GameOver scene now");
//        SceneManager.LoadScene(gameOverScenePath);
//    }

//    // Get current shot count
//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    // Clean up event listeners
//    void OnDestroy()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }
//}




//-----------------------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//public class GameManager : MonoBehaviour
//{
//    // Singleton pattern
//    public static GameManager Instance { get; private set; }

//    // Level progression data
//    [System.Serializable]
//    public class LevelData
//    {
//        public string sceneName;
//        public string nextLevelName;
//    }

//    public List<LevelData> levelSequence = new List<LevelData>();
//    private string currentLevel;

//    // Player tracking
//    [SerializeField] // Make this visible in the inspector for debugging
//    private int shotCount = 0;
//    public int maxShots = 3; // Max shots before Game Over

//    // Enemy tag
//    public string enemyTag = "Enemy";
//    public string gameOverSceneName = "_Scenes/GameOver"; // Make this configurable

//    // Debug mode
//    public bool debugMode = true;

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    void Awake()
//    {
//        Debug.Log("GameManager Awake called");

//        // Implement singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Register for scene loaded events
//            SceneManager.sceneLoaded += OnSceneLoaded;

//            // Initialize with first scene
//            currentLevel = SceneManager.GetActiveScene().name;

//            // Load saved shot count if any
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);

//            Debug.Log($"GameManager initialized. Current level: {currentLevel}, Shot count: {shotCount}");
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        Debug.Log($"Scene loaded: {scene.name}");

//        // Skip further processing if this is the GameOver scene
//        if (scene.name == gameOverSceneName || scene.name == "GameOver")
//        {
//            Debug.Log("This is the GameOver scene - not setting up level manager or progression");
//            return;
//        }

//        // Update current level if it's a game level
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == scene.name)
//            {
//                currentLevel = scene.name;
//                Debug.Log($"This is a game level: {currentLevel}");
//                break;
//            }
//        }

//        // Check for enemies and add a LevelManager if needed
//        if (FindAnyObjectByType<LevelManager>() == null)
//        {
//            if (GameObject.FindGameObjectsWithTag(enemyTag).Length > 0)
//            {
//                GameObject levelManagerObj = new GameObject("LevelManager");
//                LevelManager manager = levelManagerObj.AddComponent<LevelManager>();
//                manager.enemyTag = enemyTag;
//                Debug.Log("Created new LevelManager");
//            }
//        }
//    }

//    public void CompleteLevel()
//    {
//        Debug.Log("CompleteLevel called");

//        // Reset shot count
//        shotCount = 0;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Find next level
//        string nextLevel = "";
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevel)
//            {
//                nextLevel = level.nextLevelName;
//                break;
//            }
//        }

//        // Load next level with delay
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            Debug.Log($"Will load next level: {nextLevel} in 1 second");
//            Invoke("LoadNextLevelDelayed", 1f);
//        }
//    }

//    void LoadNextLevelDelayed()
//    {
//        // Find next level
//        string nextLevel = "";
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevel)
//            {
//                nextLevel = level.nextLevelName;
//                break;
//            }
//        }

//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            Debug.Log($"Loading next level: {nextLevel}");
//            SceneManager.LoadScene(nextLevel);
//        }
//    }

//    public void IncrementShotCount()
//    {
//        shotCount++;
//        Debug.Log($"Shot count increased to {shotCount}");

//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // IMPORTANT CHECK: If shot count reaches max, go to game over
//        if (shotCount >= maxShots)
//        {
//            Debug.Log($"Shot count {shotCount} >= maxShots {maxShots}, going to game over!");

//            // Reset shot count
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", shotCount);
//            PlayerPrefs.Save();

//            // Force a direct load of the game over scene
//            ForceGameOver();
//        }
//    }

//    // Guaranteed way to load the game over scene
//    public void ForceGameOver()
//    {
//        Debug.Log("ForceGameOver called - Loading game over scene immediately");

//        // Cancel any pending actions
//        CancelInvoke();

//        // Direct scene load for maximum reliability
//        SceneManager.LoadScene(gameOverSceneName);
//    }

//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    void OnDestroy()
//    {
//        // Unregister from scene events
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }
//}



//------------------------------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//public class GameManager : MonoBehaviour
//{
//    // Singleton pattern
//    public static GameManager Instance { get; private set; }

//    // Level progression data
//    [System.Serializable]
//    public class LevelData
//    {
//        public string sceneName;
//        public string nextLevelName;
//    }

//    public List<LevelData> levelSequence = new List<LevelData>();
//    private string currentLevel;

//    // Player tracking
//    [SerializeField] // Make this visible in the inspector for debugging
//    private int shotCount = 0;
//    public int maxShots = 3; // Max shots before Game Over

//    // Enemy tag
//    public string enemyTag = "Enemy";
//    public string gameOverSceneName = "_Scenes/GameOver"; // Make this configurable

//    // Debug mode
//    public bool debugMode = true;

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    void Awake()
//    {
//        Debug.Log("GameManager Awake called");

//        // Implement singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Register for scene loaded events
//            SceneManager.sceneLoaded += OnSceneLoaded;

//            // Initialize with first scene
//            currentLevel = SceneManager.GetActiveScene().name;

//            // Load saved shot count if any
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);

//            Debug.Log($"GameManager initialized. Current level: {currentLevel}, Shot count: {shotCount}");
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        Debug.Log($"Scene loaded: {scene.name}");

//        // Update current level if it's a game level
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == scene.name)
//            {
//                currentLevel = scene.name;
//                Debug.Log($"This is a game level: {currentLevel}");
//                break;
//            }
//        }

//        // Check for enemies and add a LevelManager if needed
//        if (FindFirstObjectByType<LevelManager>() == null)
//        {
//            if (GameObject.FindGameObjectsWithTag(enemyTag).Length > 0)
//            {
//                GameObject levelManagerObj = new GameObject("LevelManager");
//                LevelManager manager = levelManagerObj.AddComponent<LevelManager>();
//                manager.enemyTag = enemyTag;
//                Debug.Log("Created new LevelManager");
//            }
//        }
//    }

//    public void CompleteLevel()
//    {
//        Debug.Log("CompleteLevel called");

//        // Reset shot count
//        shotCount = 0;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Find next level
//        string nextLevel = "";
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevel)
//            {
//                nextLevel = level.nextLevelName;
//                break;
//            }
//        }

//        // Load next level with delay
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            Debug.Log($"Will load next level: {nextLevel} in 1 second");
//            Invoke("LoadNextLevelDelayed", 1f);
//        }
//    }

//    void LoadNextLevelDelayed()
//    {
//        // Find next level
//        string nextLevel = "";
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevel)
//            {
//                nextLevel = level.nextLevelName;
//                break;
//            }
//        }

//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            Debug.Log($"Loading next level: {nextLevel}");
//            SceneManager.LoadScene(nextLevel);
//        }
//    }

//    public void IncrementShotCount()
//    {
//        shotCount++;
//        Debug.Log($"Shot count increased to {shotCount}");

//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // IMPORTANT CHECK: If shot count reaches max, go to game over
//        if (shotCount >= maxShots)
//        {
//            Debug.Log($"Shot count {shotCount} >= maxShots {maxShots}, going to game over!");

//            // Reset shot count
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", shotCount);
//            PlayerPrefs.Save();

//            // Force a direct load of the game over scene
//            ForceGameOver();
//        }
//    }

//    // Guaranteed way to load the game over scene
//    public void ForceGameOver()
//    {
//        Debug.Log("ForceGameOver called - Loading game over scene immediately");

//        // Cancel any pending actions
//        CancelInvoke();

//        // Direct scene load for maximum reliability
//        SceneManager.LoadScene(gameOverSceneName);
//    }

//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    void OnDestroy()
//    {
//        // Unregister from scene events
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }
//}

//-----------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//public class GameManager : MonoBehaviour
//{
//    // Singleton pattern
//    public static GameManager Instance { get; private set; }

//    // Level progression data
//    [System.Serializable]
//    public class LevelData
//    {
//        public string sceneName;
//        public string nextLevelName;
//    }

//    public List<LevelData> levelSequence = new List<LevelData>();
//    private string currentLevel;

//    // Player tracking
//    private int shotCount = 0;
//    public int maxShots = 3; // Max shots before Game Over

//    // Enemy tag
//    public string enemyTag = "Enemy";

//    // Debug settings
//    public bool debugMode = true;

//    // Level completion control
//    private bool isLevelCompleted = false;

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    // Reference to the LevelManager we create
//    private GameObject levelManagerObject;

//    void Awake()
//    {
//        // Implement singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Register for scene loaded events
//            SceneManager.sceneLoaded += OnSceneLoaded;

//            // Initialize with first scene
//            currentLevel = SceneManager.GetActiveScene().name;

//            // Load saved shot count if any
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);

//            DebugLog("GameManager initialized. Current level: " + currentLevel);
//            PrintLevelSequence();
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        // Reset level completion flag
//        isLevelCompleted = false;

//        // Update current level
//        currentLevel = scene.name;
//        DebugLog("Scene loaded: " + currentLevel);

//        // Check if this is a game level (not menu, etc)
//        if (IsGameLevel(currentLevel))
//        {
//            DebugLog("This is a game level. Creating LevelManager...");
//            // Create a LevelManager for this level
//            CreateLevelManager();

//            // Start our own enemy check as a backup
//            Invoke("StartLevelCheck", 0.5f); // Delay to ensure all objects are initialized
//        }
//        else
//        {
//            DebugLog("This is NOT a game level (not in level sequence)");
//        }
//    }

//    void PrintLevelSequence()
//    {
//        if (!debugMode) return;

//        Debug.Log("==== LEVEL SEQUENCE CONFIGURATION ====");
//        foreach (LevelData level in levelSequence)
//        {
//            Debug.Log($"Level: {level.sceneName} -> Next: {level.nextLevelName}");
//        }
//        Debug.Log("=====================================");
//    }

//    void CreateLevelManager()
//    {
//        // First, check if a LevelManager already exists
//        LevelManager existingManager = FindFirstObjectByType<LevelManager>();
//        if (existingManager != null)
//        {
//            DebugLog("LevelManager already exists, not creating a new one");
//            return;
//        }

//        // Create a new GameObject for the LevelManager
//        levelManagerObject = new GameObject("LevelManager");

//        // Add the LevelManager script component
//        LevelManager levelManager = levelManagerObject.AddComponent<LevelManager>();

//        // Configure the LevelManager
//        levelManager.enemyTag = this.enemyTag;

//        DebugLog("LevelManager created and configured with tag: " + this.enemyTag);
//    }

//    private bool IsGameLevel(string sceneName)
//    {
//        // Check if the scene is in our level sequence
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == sceneName)
//                return true;
//        }

//        return false;
//    }

//    void StartLevelCheck()
//    {
//        DebugLog("Starting level completion check");
//        // Clear any previous invokes to avoid duplicates
//        CancelInvoke("CheckLevelCompletion");
//        // Start checking for level completion
//        InvokeRepeating("CheckLevelCompletion", 0.5f, 0.5f);
//    }

//    void CheckLevelCompletion()
//    {
//        // Don't check if level is already completed
//        if (isLevelCompleted)
//        {
//            DebugLog("Level already marked as completed, skipping check");
//            return;
//        }

//        // Find enemies with tag
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

//        DebugLog($"Checking level completion. Found {enemies.Length} enemies remaining.");

//        // If no enemies left, complete level
//        if (enemies.Length == 0)
//        {
//            DebugLog("No enemies found! Completing level...");
//            CancelInvoke("CheckLevelCompletion");
//            CompleteLevel();
//        }
//    }

//    // Make this public so it can be called from LevelManager
//    public void CompleteLevel()
//    {
//        // Prevent multiple calls
//        if (isLevelCompleted)
//        {
//            DebugLog("Level already completed, ignoring duplicate call");
//            return;
//        }

//        isLevelCompleted = true;
//        DebugLog("CompleteLevel called for: " + currentLevel);

//        // Prevent additional checks
//        CancelInvoke("CheckLevelCompletion");

//        // Reset shot count when level is completed
//        shotCount = 0;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Find next level
//        string nextLevel = GetNextLevelName(currentLevel);
//        DebugLog("Next level determined to be: " + (nextLevel ?? "NULL or EMPTY"));

//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            // Give a slight delay before loading next level
//            DebugLog("Setting timer to load next level in 1 second...");
//            CancelInvoke("LoadNextLevel"); // Clear any existing invokes
//            Invoke("LoadNextLevel", 1f);
//        }
//        else
//        {
//            DebugLog("WARNING: No next level found for " + currentLevel);
//        }
//    }

//    void LoadNextLevel()
//    {
//        string nextLevel = GetNextLevelName(currentLevel);
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            DebugLog("Loading next level: " + nextLevel);
//            SceneManager.LoadScene(nextLevel);
//        }
//        else
//        {
//            DebugLog("ERROR: Next level name is empty when trying to load!");
//        }
//    }

//    string GetNextLevelName(string currentLevelName)
//    {
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevelName)
//            {
//                DebugLog($"Found level in sequence: {level.sceneName} -> Next: {level.nextLevelName}");
//                return level.nextLevelName;
//            }
//        }

//        DebugLog($"WARNING: Could not find {currentLevelName} in level sequence!");
//        return "";
//    }

//    public void IncrementShotCount()
//    {
//        shotCount++;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        DebugLog($"Shot count incremented to {shotCount}/{maxShots}");

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Check if player has used all shots
//        if (shotCount >= maxShots)
//        {
//            DebugLog("Max shots reached! Going to Game Over");

//            // Reset shot count
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", shotCount);
//            PlayerPrefs.Save();

//            // IMPORTANT: Go to Game Over scene immediately
//            GoToGameOver();
//        }
//    }

//    // Separate method to ensure GameOver is triggered consistently
//    public void GoToGameOver()
//    {
//        DebugLog("GoToGameOver called - Loading GameOver scene");
//        // Cancel any pending level loads
//        CancelInvoke("LoadNextLevel");

//        // Mark level as completed to prevent other transitions
//        isLevelCompleted = true;

//        // Load game over scene
//        SceneManager.LoadScene("_Scenes/GameOver");
//    }

//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    void OnDestroy()
//    {
//        // Unregister from scene events
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    // Helper for debug logging
//    private void DebugLog(string message)
//    {
//        if (debugMode)
//        {
//            Debug.Log("[GameManager] " + message);
//        }
//    }
//}


//-------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//public class GameManager : MonoBehaviour
//{
//    // Singleton pattern
//    public static GameManager Instance { get; private set; }

//    // Level progression data
//    [System.Serializable]
//    public class LevelData
//    {
//        public string sceneName;
//        public string nextLevelName;
//    }

//    public List<LevelData> levelSequence = new List<LevelData>();
//    private string currentLevel;

//    // Player tracking
//    private int shotCount = 0;
//    public int maxShots = 3;

//    // Enemy tag
//    public string enemyTag = "Enemy";

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    // Reference to the LevelManager we create
//    private GameObject levelManagerObject;

//    void Awake()
//    {
//        // Implement singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Register for scene loaded events
//            SceneManager.sceneLoaded += OnSceneLoaded;

//            // Initialize with first scene
//            currentLevel = SceneManager.GetActiveScene().name;

//            // Load saved shot count if any
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        // Update current level
//        currentLevel = scene.name;

//        // Check if this is a game level (not menu, etc)
//        if (IsGameLevel(currentLevel))
//        {
//            // Create a LevelManager for this level
//            CreateLevelManager();

//            // Start our own enemy check as a backup
//            StartLevelCheck();
//        }
//    }

//    void CreateLevelManager()
//    {
//        // First, check if a LevelManager already exists
//        LevelManager existingManager = FindFirstObjectByType<LevelManager>();
//        if (existingManager != null)
//        {
//            // If it exists, don't create a new one
//            return;
//        }

//        // Create a new GameObject for the LevelManager
//        levelManagerObject = new GameObject("LevelManager");

//        // Add the LevelManager script component
//        LevelManager levelManager = levelManagerObject.AddComponent<LevelManager>();

//        // Configure the LevelManager
//        levelManager.enemyTag = this.enemyTag;
//    }

//    private bool IsGameLevel(string sceneName)
//    {
//        // Check if the scene is in our level sequence
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == sceneName)
//                return true;
//        }

//        return false;
//    }

//    void StartLevelCheck()
//    {
//        // Start checking for level completion
//        InvokeRepeating("CheckLevelCompletion", 1f, 0.5f);
//    }

//    void CheckLevelCompletion()
//    {
//        // Find enemies with tag
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

//        // If no enemies left, complete level
//        if (enemies.Length == 0)
//        {
//            CancelInvoke("CheckLevelCompletion");
//            CompleteLevel();
//        }
//    }

//    // Make this public so it can be called via SendMessage
//    public void CompleteLevel()
//    {
//        // Prevent multiple calls
//        CancelInvoke("CheckLevelCompletion");

//        // Reset shot count when level is completed
//        shotCount = 0;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Find next level
//        string nextLevel = GetNextLevelName(currentLevel);
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            // Give a slight delay before loading next level
//            Invoke("LoadNextLevel", 1f);
//        }
//    }

//    void LoadNextLevel()
//    {
//        string nextLevel = GetNextLevelName(currentLevel);
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            SceneManager.LoadScene(nextLevel);
//        }
//    }

//    string GetNextLevelName(string currentLevelName)
//    {
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevelName)
//                return level.nextLevelName;
//        }

//        return "";
//    }

//    public void IncrementShotCount()
//    {
//        shotCount++;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Check if player has used all shots
//        if (shotCount >= maxShots)
//        {
//            // Reset shot count
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", shotCount);
//            PlayerPrefs.Save();

//            // Go to Game Over
//            SceneManager.LoadScene("_Scenes/GameOver");
//        }
//    }

//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    void OnDestroy()
//    {
//        // Unregister from scene events
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }
//}

//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections.Generic;

//public class GameManager : MonoBehaviour
//{
//    // Singleton pattern
//    public static GameManager Instance { get; private set; }

//    // Level progression data
//    [System.Serializable]
//    public class LevelData
//    {
//        public string sceneName;
//        public string nextLevelName;
//    }

//    public List<LevelData> levelSequence = new List<LevelData>();
//    private string currentLevel;

//    // Player tracking
//    private int shotCount = 0;
//    public int maxShots = 3;

//    // Events
//    public delegate void ShotCountChanged(int newCount);
//    public static event ShotCountChanged OnShotCountChanged;

//    void Awake()
//    {
//        // Implement singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Register for scene loaded events
//            SceneManager.sceneLoaded += OnSceneLoaded;

//            // Initialize with first scene
//            currentLevel = SceneManager.GetActiveScene().name;

//            // Load saved shot count if any
//            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        // Update current level
//        currentLevel = scene.name;

//        // Check if this is a game level (not menu, etc)
//        if (IsGameLevel(currentLevel))
//        {
//            // Find all enemies in the new scene
//            StartLevelCheck();
//        }
//    }

//    private bool IsGameLevel(string sceneName)
//    {
//        // Check if the scene is in our level sequence
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == sceneName)
//                return true;
//        }

//        return false;
//    }

//    void StartLevelCheck()
//    {
//        // Start checking for level completion
//        InvokeRepeating("CheckLevelCompletion", 1f, 0.5f);
//    }

//    void CheckLevelCompletion()
//    {
//        // Find enemies with tag
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

//        // If no enemies left, complete level
//        if (enemies.Length == 0)
//        {
//            CancelInvoke("CheckLevelCompletion");
//            CompleteLevel();
//        }
//    }

//    void CompleteLevel()
//    {
//        // Reset shot count when level is completed
//        shotCount = 0;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Find next level
//        string nextLevel = GetNextLevelName(currentLevel);
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            // Give a slight delay before loading next level
//            Invoke("LoadNextLevel", 1f);
//        }
//    }

//    void LoadNextLevel()
//    {
//        string nextLevel = GetNextLevelName(currentLevel);
//        if (!string.IsNullOrEmpty(nextLevel))
//        {
//            SceneManager.LoadScene(nextLevel);
//        }
//    }

//    string GetNextLevelName(string currentLevelName)
//    {
//        foreach (LevelData level in levelSequence)
//        {
//            if (level.sceneName == currentLevelName)
//                return level.nextLevelName;
//        }

//        return "";
//    }

//    public void IncrementShotCount()
//    {
//        shotCount++;
//        PlayerPrefs.SetInt("ShotCount", shotCount);
//        PlayerPrefs.Save();

//        // Notify listeners
//        if (OnShotCountChanged != null)
//            OnShotCountChanged(shotCount);

//        // Check if player has used all shots
//        if (shotCount >= maxShots)
//        {
//            // Reset shot count
//            shotCount = 0;
//            PlayerPrefs.SetInt("ShotCount", shotCount);
//            PlayerPrefs.Save();

//            // Go to Game Over
//            SceneManager.LoadScene("_Scenes/GameOver");
//        }
//    }

//    public int GetShotCount()
//    {
//        return shotCount;
//    }

//    void OnDestroy()
//    {
//        // Unregister from scene events
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }
//}