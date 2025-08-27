// ===== UIManager.cs (Final Version) =====
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // Required for Coroutines

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In-Game HUD Elements")]
    [SerializeField] private TextMeshProUGUI shotCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Level Complete Screen")]
    [SerializeField] private GameObject levelCompletePanel;
    // --- NEW UI REFERENCES FOR SCORE TALLY ---
    [SerializeField] private TextMeshProUGUI timeBonusText;
    [SerializeField] private TextMeshProUGUI shotsBonusText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private UnityEngine.UI.Button continueButton; // Reference to the continue button

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    private bool isPaused = false;

    [Header("Settings Menu")]
    [SerializeField] private SettingsMenu settingsMenu;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenu != null) settingsMenu.gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the Level Complete screen and starts the score tally animation.
    /// </summary>
    /// <param name="scoreData">The package of score details from the GameManager.</param>
    public void ShowLevelCompleteScreen(GameManager.ScoreData scoreData)
    {
        if (levelCompletePanel == null)
        {
            Debug.LogError("Level Complete UI elements are not assigned in the UIManager!");
            return;
        }

        // Start the animation instead of just setting the text
        StartCoroutine(AnimateScoreTally(scoreData));
    }

    /// <summary>
    /// Animates the score tally on the level complete screen.
    /// </summary>
    private IEnumerator AnimateScoreTally(GameManager.ScoreData data)
    {
        // 1. Prepare the screen
        levelCompletePanel.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(false);

        // Clear previous text
        timeBonusText.text = "";
        shotsBonusText.text = "";
        finalScoreText.text = "";

        // Using WaitForSecondsRealtime is important because Time.timeScale is 0
        yield return new WaitForSecondsRealtime(0.5f);

        // 2. Show Time Bonus
        timeBonusText.text = $"Time Bonus: {data.timeTaken:F1}s  x {data.timeMultiplier:F1}";
        // Using "F1" formats the float to one decimal place
        // TODO: Play a sound effect here!
        // You can play a sound effect here! e.g., SoundManager.Instance.PlaySFX(tallySoundIndex);


        yield return new WaitForSecondsRealtime(1.0f);

        // 3. Show Shots Bonus
        shotsBonusText.text = $"Shots Left Bonus: {data.shotsLeft}  x {data.shotMultiplier:F1}";
        // TODO: Play another sound effect

        yield return new WaitForSecondsRealtime(1.0f);

        // 4. Show the Final Score
        finalScoreText.text = $"Final Score: {data.finalScore:N0}";
        // TODO: Play a final, bigger sound effect!

        yield return new WaitForSecondsRealtime(0.5f);

        // 5. Show the continue button
        if (continueButton != null) continueButton.gameObject.SetActive(true);
    }

    public void UpdateShotCount(int currentShots, int maxShots)
    {
        if (shotCountText != null)
        {
            int shotsLeft = maxShots - currentShots;
            shotCountText.text = "Shots Left: " + shotsLeft;
        }
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

    public void OnContinueButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProceedToNextLevel();
        }
    }

    // --- PAUSE & SETTINGS MENU LOGIC ---

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuPanel.SetActive(false);
        }
    }

    public void OnResumeButtonPressed()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    public void OnMainMenuButtonPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("_Scenes/MainMenu"); // Make sure this scene name is correct
    }

    public void OpenSettingsMenu()
    {
        pauseMenuPanel.SetActive(false);
        settingsMenu.gameObject.SetActive(true);
        settingsMenu.RefreshUI();
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.gameObject.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }
}