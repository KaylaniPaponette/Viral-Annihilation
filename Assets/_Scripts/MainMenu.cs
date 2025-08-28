// ===== MainMenu.cs (Updated for Player Name UI) =====
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Required for TextMeshPro elements
using System.Collections; // Required for Coroutines

public class MainMenu : MonoBehaviour
{
    [Header("Settings Menu")]
    [SerializeField] private SettingsMenu settingsMenu;

    // --- NEW UI REFERENCES ---
    [Header("Player Name UI")]
    [SerializeField] private GameObject playerNamePanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private UnityEngine.UI.Button submitNameButton;
    // The signInButtonText field has been removed

    private const string PlayerNamePrefsKey = "PlayerName";

    void Start()
    {
        // Ensure menus are in the correct state at start
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(false);
            settingsMenu.RefreshUI();
        }
        // --- NEW ---
        if (playerNamePanel != null)
        {
            playerNamePanel.SetActive(false);
        }

        // The logic to update the button text has been removed.
    }

    #region Player Name Methods
    // --- NEW ---
    public void OpenPlayerNamePanel()
    {
        if (playerNamePanel == null) return;

        playerNamePanel.SetActive(true);
        statusText.text = ""; // Clear status text
        submitNameButton.interactable = true; // Ensure button is enabled

        // Pre-fill the input field with the saved name, if it exists
        if (PlayerPrefs.HasKey(PlayerNamePrefsKey))
        {
            nameInputField.text = PlayerPrefs.GetString(PlayerNamePrefsKey);
        }
        else
        {
            nameInputField.text = "";
        }
    }

    // --- NEW ---
    public void ClosePlayerNamePanel()
    {
        if (playerNamePanel != null)
        {
            playerNamePanel.SetActive(false);
        }
    }

    // --- NEW ---
    public void OnSubmitPlayerName()
    {
        string newName = nameInputField.text;

        // Basic validation
        if (string.IsNullOrWhiteSpace(newName))
        {
            statusText.text = "Name cannot be empty!";
            return;
        }

        // Disable button to prevent spamming
        submitNameButton.interactable = false;
        statusText.text = "Saving...";

        // Call the LootLockerManager
        LootLockerManager.Instance.SetPlayerName(newName, (success) =>
        {
            // This code runs after LootLocker responds
            if (success)
            {
                statusText.text = "Name saved successfully!";
                // The line to update the button text has been removed.
                StartCoroutine(ClosePanelAfterDelay(1.5f));
            }
            else
            {
                statusText.text = "Error: Could not save name.";
                submitNameButton.interactable = true; // Re-enable button on failure
            }
        });
    }

    // --- NEW ---
    private IEnumerator ClosePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClosePlayerNamePanel();
    }
    #endregion

    #region Existing Methods
    public void OpenSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(true);
            settingsMenu.RefreshUI();
        }
    }

    public void CloseSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameState();
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot reset state.");
        }
        SceneManager.LoadScene("_Scenes/Level01");
    }

    public void FromAboutToMain()
    {
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void FromMainToAbout()
    {
        SceneManager.LoadScene("_Scenes/About");
    }
    #endregion
}
