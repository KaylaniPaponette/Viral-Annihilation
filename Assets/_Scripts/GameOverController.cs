

// ===== GameOverController.cs =====
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    // Delay before allowing input
    public float inputDelay = 3f;
    private float timer = 0f;
    private bool canTransition = false;

    // UI References
    public Button restartButton;
    public Button quitButton;

    void Start()
    {
        Debug.Log("GameOver controller started - waiting for delay");

        // Disable buttons initially
        if (restartButton != null)
        {
            restartButton.interactable = false;
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.interactable = false;
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        // Reset any PlayerPrefs shot count
        PlayerPrefs.SetInt("ShotCount", 0);
        PlayerPrefs.Save();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!canTransition && timer >= inputDelay)
        {
            canTransition = true;
            Debug.Log("GameOver controller now allowing transition");

            // Enable buttons
            if (restartButton != null) restartButton.interactable = true;
            if (quitButton != null) quitButton.interactable = true;
        }

        // Check for key press
        if (canTransition && Input.anyKeyDown)
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        if (!canTransition) return;

        Debug.Log("Restarting game from GameOver scene");
        canTransition = false;
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void QuitGame()
    {
        if (!canTransition) return;

        Debug.Log("Quitting game from GameOver scene");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}

//-----------------------------------------------------------------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class GameOverController : MonoBehaviour
//{
//    // Delay before allowing any input or automatic transitions
//    public float inputDelay = 3f;
//    private float timer = 0f;
//    private bool canTransition = false;

//    // Optional reference to a restart button
//    public Button restartButton;

//    void Start()
//    {
//        Debug.Log("GameOver scene started - waiting for " + inputDelay + " seconds before allowing transitions");

//        // Disable button initially if it exists
//        if (restartButton != null)
//        {
//            restartButton.interactable = false;
//        }

//        // Destroy any existing GameManager to reset game state
//        if (GameManager.Instance != null)
//        {
//            Debug.Log("Destroying GameManager in GameOver scene");
//            Destroy(GameManager.Instance.gameObject);
//        }

//        // Reset the shot count in PlayerPrefs
//        PlayerPrefs.SetInt("ShotCount", 0);
//        PlayerPrefs.Save();
//    }

//    void Update()
//    {
//        // Update timer
//        if (!canTransition)
//        {
//            timer += Time.deltaTime;
//            if (timer >= inputDelay)
//            {
//                canTransition = true;
//                Debug.Log("GameOver scene now allows transitions");

//                // Enable button if it exists
//                if (restartButton != null)
//                {
//                    restartButton.interactable = true;
//                }
//            }
//        }

//        // Check for any key press after delay
//        if (canTransition && Input.anyKeyDown)
//        {
//            RestartGame();
//        }
//    }

//    // Public method to restart game (can be called by UI button)
//    public void RestartGame()
//    {
//        if (!canTransition) return;

//        Debug.Log("Restarting game from GameOver scene - loading Level1");
//        SceneManager.LoadScene("MainMenu");
//    }
//}