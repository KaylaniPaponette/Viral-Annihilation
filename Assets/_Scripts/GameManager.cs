//=====GameManager.cs (with Pause Functionality)=====
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // --- TIMER VARIABLES ---
    private float levelTimer = 0f;
    private bool isTimerRunning = false;

    // Shot tracking
    [SerializeField] private int shotCount = 0;
    public int maxShots = 3;

    [Header("Music Settings")]
    public int defaultBgmIndex = 0;

    // Game state
    private bool isTransitioningToGameOver = false;
    private bool isTransitioningToNextLevel = false;

    [Header("Scoring")]
    [SerializeField] public int baseScore = 10000;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized");
            shotCount = PlayerPrefs.GetInt("ShotCount", 0);
            Debug.Log($"Starting with shot count: {shotCount}");
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        string newSceneName = scene.name;
        Debug.Log($"Scene loaded: {newSceneName}");

        bool isGameplayLevel = IsLevelInSequence(newSceneName);

        if (isGameplayLevel)
        {
            if (newSceneName != this.currentLevel)
            {
                Debug.Log($"New level '{newSceneName}' detected (was '{this.currentLevel}'). Resetting timer.");
                levelTimer = 0f;
            }
            isTimerRunning = true;
        }
        else
        {
            isTimerRunning = false;
        }

        LevelData currentLevelData = GetLevelData(newSceneName);
        if (currentLevelData != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(currentLevelData.bgmIndex);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(defaultBgmIndex);
        }

        if (newSceneName == "GameOver" || scene.path.Contains("/GameOver"))
        {
            Debug.Log("GameOver scene detected - resetting shot count");
            shotCount = 0;
            PlayerPrefs.SetInt("ShotCount", 0);
            PlayerPrefs.Save();
            isTransitioningToGameOver = false;
            isTransitioningToNextLevel = false;
        }
        else if (isGameplayLevel)
        {
            if (UIManager.Instance != null) UIManager.Instance.UpdateShotCount(shotCount, maxShots);
            isTransitioningToNextLevel = false;
            enemyCheckTimer = 0;
            Invoke("InitialEnemyCheck", 0.2f);
        }
        else
        {
            Debug.Log($"Scene {newSceneName} is not in level sequence - not checking for enemies");
        }
        this.currentLevel = newSceneName;
    }

    LevelData GetLevelData(string sceneName)
    {
        foreach (LevelData level in levelSequence)
        {
            if (level.sceneName == sceneName || level.sceneName.EndsWith("/" + sceneName))
            {
                return level;
            }
        }
        return null;
    }

    void InitialEnemyCheck()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Debug.Log($"Initial enemy check for {currentLevel}: Found {enemies.Length} enemies with tag '{enemyTag}'");
        if (enemies.Length == 0)
        {
            Debug.LogWarning($"No enemies found with tag '{enemyTag}'. Make sure your enemies have this tag.");
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
            if (level.sceneName == sceneName || level.sceneName.EndsWith("/" + sceneName) || sceneName.EndsWith("/" + level.sceneName))
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (isTransitioningToGameOver || isTransitioningToNextLevel)
            return;

        if (isTimerRunning && UIManager.Instance != null)
        {
            levelTimer += Time.deltaTime;
            UIManager.Instance.UpdateTimer(levelTimer);
        }

        if (IsLevelInSequence(currentLevel))
        {
            enemyCheckTimer += Time.deltaTime;
            if (enemyCheckTimer >= enemyCheckInterval)
            {
                enemyCheckTimer = 0;
                CheckForEnemies();
            }
        }
    }

    void CheckForEnemies()
    {
        if (GameObject.FindGameObjectsWithTag(enemyTag).Length == 0)
        {
            Debug.Log("All enemies destroyed - completing level");
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        if (isTransitioningToNextLevel) return;

        isTimerRunning = false;
        isTransitioningToNextLevel = true;

        // NEW: Pause the game when the level is complete
        Time.timeScale = 0f;

        // Scoring Logic
        float timeMultiplier = Mathf.Max(1, 100 / levelTimer);
        int shotsLeft = maxShots - shotCount;
        float shotMultiplier = 1 + (shotsLeft * 0.5f);
        int finalScore = Mathf.RoundToInt(baseScore * timeMultiplier * shotMultiplier);
        Debug.Log($"LEVEL COMPLETE! Time: {levelTimer:F2}s, Shots: {shotCount}. Final Score: {finalScore}");

        // SEND 'finalScore' to LootLocker
        // For now, we can just save it or add it to a total score
        PlayerPrefs.SetInt("TotalScore", PlayerPrefs.GetInt("TotalScore", 0) + finalScore);

        // Show the level complete screen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelCompleteScreen(finalScore);
        }
        else
        {
            Debug.LogError("UIManager not found! Loading next level directly.");
            ProceedToNextLevel();
        }
    }

    public void ProceedToNextLevel()
    {
        // NEW: Unpause the game before loading the next level
        Time.timeScale = 1f;

        shotCount = 0;
        PlayerPrefs.SetInt("ShotCount", 0);
        PlayerPrefs.Save();
        if (OnShotCountChanged != null) OnShotCountChanged(shotCount);

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
        foreach (LevelData level in levelSequence)
        {
            if (level.sceneName == currentLevelName || level.sceneName.EndsWith("/" + currentLevelName) || currentLevelName.EndsWith("/" + level.sceneName))
            {
                return level.nextLevelName;
            }
        }
        Debug.LogWarning($"No match found for {currentLevelName} in level sequence!");
        return "";
    }

    public void IncrementShotCount()
    {
        if (isTransitioningToGameOver || isTransitioningToNextLevel) return;

        shotCount++;
        Debug.Log($"Shot count increased to {shotCount}/{maxShots}");
        PlayerPrefs.SetInt("ShotCount", shotCount);
        PlayerPrefs.Save();

        if (OnShotCountChanged != null) OnShotCountChanged(shotCount);
        if (UIManager.Instance != null) UIManager.Instance.UpdateShotCount(shotCount, maxShots);

        if (shotCount >= maxShots)
        {
            Debug.Log("GAME OVER - Max shots reached");
            GoToGameOver();
        }
    }

    public void GoToGameOver()
    {
        if (isTransitioningToGameOver) return;
        isTransitioningToGameOver = true;
        Debug.Log("Going to GameOver scene");
        Invoke("LoadGameOverScene", 0.1f);
    }

    private void LoadGameOverScene()
    {
        Debug.Log("Loading GameOver scene now");
        SceneManager.LoadScene(gameOverScenePath);
    }

    public int GetShotCount()
    {
        return shotCount;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [ContextMenu("Force Check For Enemies")]
    public void ForceCheckForEnemies()
    {
        InitialEnemyCheck();
    }

    [ContextMenu("Force Complete Level")]
    public void ForceCompleteLevel()
    {
        CompleteLevel();
    }
}