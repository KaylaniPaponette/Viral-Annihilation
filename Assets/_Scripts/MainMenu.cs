// ===== MainMenu.cs (UPDATED) =====
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // --- NEW ---
    [Header("Settings Menu")]
    [SerializeField] private SettingsMenu settingsMenu;

    // --- NEW ---
    void Start()
    {
        // Ensure the settings menu is hidden when the main menu loads
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(false);
            settingsMenu.RefreshUI();
        }
    }

    // --- NEW ---
    public void OpenSettingsMenu()
    {
        // Show the settings menu
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(true);
            // Call LoadSettings to ensure values are up-to-date
            settingsMenu.RefreshUI();
        }
    }

    // --- NEW ---
    public void CloseSettingsMenu()
    {
        // Hide the settings menu
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
}