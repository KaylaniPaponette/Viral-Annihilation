// ===== WinScreenController.cs =====
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenController : MonoBehaviour
{
    public float inputDelay = 3f;
    public float leaderboardRefreshDelay = 2.0f; // Delay to allow server to process score

    private float timer = 0f;
    private bool canTransition = false;

    public Button restartButton;
    public Button quitButton;

    void Start()
    {
        Debug.Log("WinScreen controller started - waiting for delay");

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.interactable = false;
            restartButton.onClick.AddListener(RestartGame);
        }
        if (quitButton != null)
        {
            quitButton.interactable = false;
            quitButton.onClick.AddListener(QuitGame);
        }
        SubmitAndRefreshLeaderboard();

        // NOTE: PlayerPrefs reset logic has been moved to the MainMenu/GameManager to ensure it happens

        //// Reset game state for the next run
        //PlayerPrefs.SetInt("ShotCount", 0);
        //PlayerPrefs.SetInt("TotalScore", 0); // Reset the total score
        //PlayerPrefs.Save();
    }

    void SubmitAndRefreshLeaderboard()
    {
        // Tell the persistent GameManager to submit the final score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitTotalScore();
        }

        // Replace the deprecated method call with the recommended one
        LeaderboardDisplay leaderboard = Object.FindFirstObjectByType<LeaderboardDisplay>();
        if (leaderboard != null)
        {
            // Tell the leaderboard to fetch scores after a delay.
            // This gives the server time to process our new score.
            leaderboard.Invoke("FetchAndDisplayScores", leaderboardRefreshDelay);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!canTransition && timer >= inputDelay)
        {
            canTransition = true;
            Debug.Log("WinScreen controller now allowing transition");

            if (restartButton != null) restartButton.interactable = true;
            if (quitButton != null) quitButton.interactable = true;
        }

        //// Check for key press to restart
        //if (canTransition && Input.anyKeyDown)
        //{
        //    RestartGame();
        //}
    }


    public void RestartGame()
    {
        if (!canTransition) return;
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void QuitGame()
    {
        if (!canTransition) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}