// ===== UIManager.cs (Final Corrected Version) =====
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In-Game HUD Elements")]
    [SerializeField] private TextMeshProUGUI shotCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelIndicatorText;

    [Header("Level Complete Screen")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI baseScoreLabel;
    [SerializeField] private TextMeshProUGUI baseScoreValue;
    [SerializeField] private TextMeshProUGUI timeBonusLabel;
    [SerializeField] private TextMeshProUGUI timeBonusValue;
    [SerializeField] private TextMeshProUGUI shotsBonusLabel;
    [SerializeField] private TextMeshProUGUI shotsBonusValue;
    [SerializeField] private TextMeshProUGUI finalScoreLabel;
    [SerializeField] private TextMeshProUGUI finalScoreValue;
    [SerializeField] private UnityEngine.UI.Button continueButton;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    private bool isPaused = false;

    [Header("Leaderboard Panel")]
    [SerializeField] private GameObject leaderboardPanel;

    [Header("HUD Sensitivity Reference")]
    [Tooltip("Assign the sensitivity slider that is showing through the menus here.")]
    [SerializeField] private GameObject hudSensitivitySlider; // Reference to the object appearing in the background

    [Header("Quit Confirmation")]
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Settings Menu")]
    [SerializeField] private SettingsMenu settingsMenu;

    [Header("Tally Sound Effects")]
    [Tooltip("The index of the looping 'tick' sound in the SoundManager.")]
    [SerializeField] private int scoreTickSfxIndex;
    [Tooltip("The index of the final 'thud' sound in the SoundManager.")]
    [SerializeField] private int scoreCompleteSfxIndex;
    [Tooltip("The time between each 'tick' sound while numbers are counting. Smaller is faster.")]
    [SerializeField] private float timeBetweenTicks = 0.05f;
    [Tooltip("Controls the playback speed of the ticking sound. 1 is normal, 2 is double speed.")]
    [SerializeField] private float scoreTickPitch = 1.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenu != null) settingsMenu.gameObject.SetActive(false);
    }

    public void ShowLevelCompleteScreen(GameManager.ScoreData scoreData)
    {
        if (levelCompletePanel == null) return;
        StartCoroutine(AnimateScoreTally(scoreData));
    }

    private IEnumerator AnimateScoreTally(GameManager.ScoreData data)
    {
        levelCompletePanel.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(false);

        baseScoreLabel.text = "";
        timeBonusLabel.text = "";
        shotsBonusLabel.text = "";
        finalScoreLabel.text = "";
        baseScoreValue.text = "";
        timeBonusValue.text = "";
        shotsBonusValue.text = "";
        finalScoreValue.text = "";

        yield return new WaitForSecondsRealtime(0.5f);

        baseScoreLabel.text = "Level Bonus";
        yield return StartCoroutine(AnimateNumber(baseScoreValue, "", 0, data.baseScore, 0.75f, false));
        yield return new WaitForSecondsRealtime(0.5f);

        timeBonusLabel.text = "Time Bonus";
        string timePrefix = $"x {data.timeMultiplier:F1} = ";
        yield return StartCoroutine(AnimateNumber(timeBonusValue, timePrefix, 0, data.timeBonusPoints, 1.25f, false));
        yield return new WaitForSecondsRealtime(0.5f);

        shotsBonusLabel.text = "Shots Bonus";
        string shotsPrefix = $"x {data.shotsLeft} = ";
        yield return StartCoroutine(AnimateNumber(shotsBonusValue, shotsPrefix, 0, data.shotsBonusPoints, 1.0f, false));
        yield return new WaitForSecondsRealtime(0.75f);

        finalScoreLabel.text = "Final Score";
        yield return StartCoroutine(AnimateNumber(finalScoreValue, "", 0, data.finalScore, 1.5f, true));

        yield return new WaitForSecondsRealtime(0.5f);
        if (continueButton != null) continueButton.gameObject.SetActive(true);
    }

    private IEnumerator AnimateNumber(TextMeshProUGUI textElement, string prefix, int startValue, int endValue, float duration, bool playCompleteSound)
    {
        if (endValue == 0)
        {
            textElement.text = $"{prefix}0";
            yield break;
        }

        float timer = 0f;
        float nextTickTime = 0f;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.sfxSource.pitch = scoreTickPitch;
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            int currentValue = (int)Mathf.Lerp(startValue, endValue, progress);
            textElement.text = $"{prefix}{currentValue:N0}";

            if (timer > nextTickTime)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(scoreTickSfxIndex);
                }
                nextTickTime += timeBetweenTicks;
            }

            yield return null;
        }

        textElement.text = $"{prefix}{endValue:N0}";

        if (playCompleteSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.sfxSource.pitch = 1.0f;
            SoundManager.Instance.PlaySFX(scoreCompleteSfxIndex);
        }
    }

    public void UpdateShotCount(int currentShots, int maxShots)
    {
        if (shotCountText != null) shotCountText.text = "Shots Left: " + (maxShots - currentShots);
    }

    public void UpdateTimer(float timeInSeconds)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void UpdateLevelIndicator(int current, int total)
    {
        if (levelIndicatorText != null)
        {
            // The :00 ensures it always shows two digits (e.g., 01 instead of 1)
            levelIndicatorText.text = $"Level {current:00} / {total:00}";
        }
    }

    public void OnContinueButtonPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.ProceedToNextLevel();
    }

    /// <summary>
    /// Helper method to toggle gameplay HUD elements (like the sensitivity slider)
    /// </summary>
    private void SetGameplayHUDVisible(bool visible)
    {
        if (hudSensitivitySlider != null)
        {
            hudSensitivitySlider.SetActive(visible);
        }
    }
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pauseMenuPanel.SetActive(isPaused);

        // Efficiency: Hide/Show the HUD slider based on pause state
        SetGameplayHUDVisible(!isPaused);

        if (!isPaused)
        {
            quitConfirmationPanel.SetActive(false);
            settingsMenu.gameObject.SetActive(false);
            leaderboardPanel.SetActive(false);
        }
    }

    public void OnResumeButtonPressed() => TogglePauseMenu();

    public void OnMainMenuButtonPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void OpenSettingsMenu()
    {
        pauseMenuPanel.SetActive(false);
        settingsMenu.gameObject.SetActive(true);
        settingsMenu.RefreshUI();

        // Ensure HUD is hidden while in settings
        SetGameplayHUDVisible(false);
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.gameObject.SetActive(false);
        pauseMenuPanel.SetActive(true);
        // HUD remains hidden because pause menu is still active
    }

    public void OpenLeaderboardFromPause()
    {
        // Hide the pause menu and show the leaderboard
        pauseMenuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);

        // Refresh the scores using your existing script
        LeaderboardDisplay ld = leaderboardPanel.GetComponentInChildren<LeaderboardDisplay>();
        if (ld != null)
        {
            ld.FetchAndDisplayScores();
        }
    }

    public void CloseLeaderboardToPause()
    {
        // Hide leaderboard and bring back the pause menu
        leaderboardPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void OpenQuitConfirmation()
    {
        pauseMenuPanel.SetActive(false);
        quitConfirmationPanel.SetActive(true);

        // Hide HUD explicitly when confirmation opens
        SetGameplayHUDVisible(false);
    }

    public void CloseQuitConfirmation()
    {
        quitConfirmationPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        // HUD remains hidden because we returned to the Pause Menu
    }

    public void ConfirmQuitToGameOver()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToGameOver(); // GameManager handles the scene change
        }
        else
        {
            SceneManager.LoadScene("_Scenes/GameOver");
        }
    }
}





//// ===== UIManager.cs (Final Version) =====
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using TMPro;
//using System.Collections; // Required for Coroutines

//public class UIManager : MonoBehaviour
//{
//    public static UIManager Instance { get; private set; }

//    [Header("In-Game HUD Elements")]
//    [SerializeField] private TextMeshProUGUI shotCountText;
//    [SerializeField] private TextMeshProUGUI timerText;

//    [Header("Level Complete Screen")]
//    [SerializeField] private GameObject levelCompletePanel;
//    // --- NEW UI REFERENCES FOR SCORE TALLY ---
//    [SerializeField] private TextMeshProUGUI timeBonusText;
//    [SerializeField] private TextMeshProUGUI shotsBonusText;
//    [SerializeField] private TextMeshProUGUI finalScoreText;
//    [SerializeField] private UnityEngine.UI.Button continueButton; // Reference to the continue button

//    [Header("Sound Effect Indexes")]
//    [Tooltip("The sound that plays for each bonus item tally.")]
//    public int tallySoundIndex;
//    [Tooltip("The sound that plays when the final score is revealed.")]
//    public int finalScoreSoundIndex;

//    [Header("Pause Menu")]
//    [SerializeField] private GameObject pauseMenuPanel;
//    private bool isPaused = false;

//    [Header("Settings Menu")]
//    [SerializeField] private SettingsMenu settingsMenu;

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void Start()
//    {
//        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
//        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
//        if (settingsMenu != null) settingsMenu.gameObject.SetActive(false);
//    }

//    /// Activates the Level Complete screen and starts the score tally animation.
//    /// <param name="scoreData">The package of score details from the GameManager.</param>
//    public void ShowLevelCompleteScreen(GameManager.ScoreData scoreData)
//    {
//        if (levelCompletePanel == null)
//        {
//            Debug.LogError("Level Complete UI elements are not assigned in the UIManager!");
//            return;
//        }

//        // Start the animation instead of just setting the text
//        StartCoroutine(AnimateScoreTally(scoreData));
//    }

//    /// Animates the score tally on the level complete screen.
//    private IEnumerator AnimateScoreTally(GameManager.ScoreData data)
//    {
//        // 1. Prepare the screen
//        levelCompletePanel.SetActive(true);
//        if (continueButton != null) continueButton.gameObject.SetActive(false);

//        // Clear previous text
//        timeBonusText.text = "";
//        shotsBonusText.text = "";
//        finalScoreText.text = "";

//        yield return new WaitForSecondsRealtime(0.5f);

//        // 2. Show Time Bonus
//        timeBonusText.text = $"Time Bonus: {data.timeTaken:F1}s  x {data.timeMultiplier:F1}";
//        // --- Play the tally sound ---
//        if (SoundManager.Instance != null)
//        {
//            SoundManager.Instance.PlaySFX(tallySoundIndex);
//        }

//        yield return new WaitForSecondsRealtime(1.0f);

//        // 3. Show Shots Bonus
//        shotsBonusText.text = $"Shots Left Bonus: {data.shotsLeft}  x {data.shotMultiplier:F1}";
//        // --- Play the tally sound again ---
//        if (SoundManager.Instance != null)
//        {
//            SoundManager.Instance.PlaySFX(tallySoundIndex);
//        }

//        yield return new WaitForSecondsRealtime(1.0f);

//        // 4. Show the Final Score
//        finalScoreText.text = $"Final Score: {data.finalScore:N0}";
//        // --- Play the final, bigger sound effect! ---
//        if (SoundManager.Instance != null)
//        {
//            SoundManager.Instance.PlaySFX(finalScoreSoundIndex);
//        }

//        yield return new WaitForSecondsRealtime(0.5f);

//        // 5. Show the continue button
//        if (continueButton != null) continueButton.gameObject.SetActive(true);
//    }

//    public void UpdateShotCount(int currentShots, int maxShots)
//    {
//        if (shotCountText != null)
//        {
//            int shotsLeft = maxShots - currentShots;
//            shotCountText.text = "Shots Left: " + shotsLeft;
//        }
//    }

//    public void UpdateTimer(float timeInSeconds)
//    {
//        if (timerText != null)
//        {
//            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
//            int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
//            timerText.text = $"Time: {minutes:00}:{seconds:00}";
//        }
//    }

//    public void OnContinueButtonPressed()
//    {
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.ProceedToNextLevel();
//        }
//    }

//    // --- PAUSE & SETTINGS MENU LOGIC ---

//    public void TogglePauseMenu()
//    {
//        isPaused = !isPaused;
//        if (isPaused)
//        {
//            Time.timeScale = 0f;
//            pauseMenuPanel.SetActive(true);
//        }
//        else
//        {
//            Time.timeScale = 1f;
//            pauseMenuPanel.SetActive(false);
//        }
//    }

//    public void OnResumeButtonPressed()
//    {
//        isPaused = false;
//        Time.timeScale = 1f;
//        pauseMenuPanel.SetActive(false);
//    }

//    public void OnMainMenuButtonPressed()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene("_Scenes/MainMenu"); // Make sure this scene name is correct
//    }

//    public void OpenSettingsMenu()
//    {
//        pauseMenuPanel.SetActive(false);
//        settingsMenu.gameObject.SetActive(true);
//        settingsMenu.RefreshUI();
//    }

//    public void CloseSettingsMenu()
//    {
//        settingsMenu.gameObject.SetActive(false);
//        pauseMenuPanel.SetActive(true);
//    }
//}