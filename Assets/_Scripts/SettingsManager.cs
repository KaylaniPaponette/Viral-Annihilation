// ===== SettingsManager.cs =====
using UnityEngine;

public static class SettingsManager
{
    // We only need the key for sensitivity now
    private const string DragSensitivityKey = "DragSensitivity";

    // The current sensitivity value that other scripts will read
    public static float DragSensitivity { get; private set; }

    static SettingsManager()
    {
        LoadSettings();
    }

    public static void LoadSettings()
    {
        // Load the sensitivity value, defaulting to 0.5f (50%)
        DragSensitivity = PlayerPrefs.GetFloat(DragSensitivityKey, 0.5f);
    }

    public static void SaveSettings(float sensitivity)
    {
        DragSensitivity = sensitivity;
        PlayerPrefs.SetFloat(DragSensitivityKey, sensitivity);
        PlayerPrefs.Save();
    }
}