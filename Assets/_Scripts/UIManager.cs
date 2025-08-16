// ===== UIManager.cs =====
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class UIManager : MonoBehaviour
{
    // A Singleton instance makes it easy for other scripts to access this UIManager
    public static UIManager Instance { get; private set; }

    [Header("In-Game HUD Elements")]
    [SerializeField] private TextMeshProUGUI shotCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Level Complete Screen")]
    // A reference to the parent panel of the level complete screen
    [SerializeField] private GameObject levelCompletePanel;
    // A reference to the text that will display the final score
    [SerializeField] private TextMeshProUGUI finalScoreText;

    // NEW
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    private bool isPaused = false;

    void Awake()
    {
        // Standard Singleton setup
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
        // Ensure the Level Complete screen is hidden when the scene first loads.
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        // Ensure the Pause Menu is hidden at the start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    // NEW: A public method that can be called by a pause button
    public void TogglePauseMenu()
    {
        isPaused = !isPaused; // Invert the paused state

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    // NEW: Helper method to handle the logic for pausing
    private void PauseGame()
    {
        Time.timeScale = 0f; // This freezes game time!
        pauseMenuPanel.SetActive(true);
    }

    // NEW: Helper method to handle the logic for resuming
    private void ResumeGame()
    {
        Time.timeScale = 1f; // This unfreezes game time back to normal.
        pauseMenuPanel.SetActive(false);
    }

    // NEW: This method will be called by the "Resume" button in the pause menu
    public void OnResumeButtonPressed()
    {
        // We can just call the ResumeGame method directly
        isPaused = false;
        ResumeGame();
    }

    // NEW: This method will be called by the "Main Menu" button
    public void OnMainMenuButtonPressed()
    {
        // IMPORTANT: Always reset timeScale before leaving a scene.
        Time.timeScale = 1f;
        // Replace "MainMenu" with the actual name of your main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    /// Updates the shot count text on the HUD.
    /// Called by the GameManager whenever the shot count changes.

    public void UpdateShotCount(int currentShots, int maxShots)
    {
        if (shotCountText != null)
        {
            int shotsLeft = maxShots - currentShots;
            shotCountText.text = "Shots Left: " + shotsLeft;
        }
    }

    /// Updates the timer text on the HUD.
    /// Called by the GameManager every frame while the timer is running.

    public void UpdateTimer(float timeInSeconds)
    {
        if (timerText != null)
        {
            // Calculate minutes and seconds from the total time
            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60F);

            // Format the string to always show two digits for minutes and seconds (e.g., 01:05)
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Activates the Level Complete screen and displays the final score.
    /// Called by the GameManager when all enemies are defeated.
    /// </summary>
    /// <param name="score">The final calculated score for the level.</param>
    public void ShowLevelCompleteScreen(int score)
    {
        // Check if the UI elements have been assigned in the Inspector
        if (levelCompletePanel == null || finalScoreText == null)
        {
            Debug.LogError("Level Complete UI elements are not assigned in the UIManager!");
            return;
        }

        // Activate the parent panel to show the screen
        levelCompletePanel.SetActive(true);
        // Set the score text, using N0 for formatting the number with commas for readability (e.g., 12,345)
        finalScoreText.text = "Score: " + score.ToString("N0");
    }
    /// <summary>
    /// This public method is called by the "Continue" button's OnClick event.
    /// It acts as a bridge to the GameManager singleton.
    /// </summary>
    public void OnContinueButtonPressed()
    {
        // Find the active GameManager instance and tell it to proceed.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProceedToNextLevel();
        }
    }

}