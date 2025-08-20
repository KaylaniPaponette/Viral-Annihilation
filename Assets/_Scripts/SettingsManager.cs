// ===== SettingsManager.cs (UPGRADED) =====
using UnityEngine;
using UnityEngine.Audio;

public static class SettingsManager
{
    // PlayerPrefs Keys
    private const string DragSensitivityKey = "DragSensitivity";
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string IsMutedKey = "IsMuted";

    // Static properties to hold current settings
    public static float DragSensitivity { get; private set; }
    public static float MasterVolume { get; private set; }
    public static float MusicVolume { get; private set; }
    public static float SFXVolume { get; private set; }
    public static bool IsMuted { get; private set; }

    // Static constructor runs once when the class is first accessed
    static SettingsManager()
    {
        LoadSettings();
    }

    public static void LoadSettings()
    {
        // Load all values from PlayerPrefs, with defaults
        DragSensitivity = PlayerPrefs.GetFloat(DragSensitivityKey, 1.0f);
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1.0f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1.0f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1.0f);
        IsMuted = PlayerPrefs.GetInt(IsMutedKey, 0) == 1;
        Debug.Log("SettingsManager: All settings loaded from PlayerPrefs.");
    }

    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat(DragSensitivityKey, DragSensitivity);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.SetInt(IsMutedKey, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("SettingsManager: All settings saved.");
    }

    // A single, reusable method to apply audio settings to any mixer
    public static void ApplyAudioSettings(AudioMixer mixer)
    {
        if (mixer == null) return;

        // Apply mute state first
        if (IsMuted)
        {
            mixer.SetFloat("MasterVolume", -80f);
        }
        else
        {
            // If not muted, apply the master volume
            mixer.SetFloat("MasterVolume", Mathf.Log10(MasterVolume) * 20);
        }

        // Apply individual music and SFX volumes
        mixer.SetFloat("MusicVolume", Mathf.Log10(MusicVolume) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(SFXVolume) * 20);

        Debug.Log("SettingsManager: Audio settings applied to mixer.");
    }
    //public static void ApplyAudioSettings(AudioMixer mixer)
    //{
    //    if (mixer == null)
    //    {
    //        Debug.LogError("SETTINGS MANAGER ERROR: Attempted to apply audio settings, but the provided AudioMixer was NULL!");
    //        return;
    //    }

    //    Debug.Log($"SETTINGS MANAGER: Applying settings to mixer '{mixer.name}'. Master Vol: {MasterVolume}, Muted: {IsMuted}");

    //    // Apply mute state first
    //    if (IsMuted)
    //    {
    //        mixer.SetFloat("MasterVolume", -80f);
    //    }
    //    else
    //    {
    //        mixer.SetFloat("MasterVolume", Mathf.Log10(MasterVolume) * 20);
    //    }

    //    mixer.SetFloat("MusicVolume", Mathf.Log10(MusicVolume) * 20);
    //    mixer.SetFloat("SFXVolume", Mathf.Log10(SFXVolume) * 20);
    //}
    // Public methods to change settings from UI scripts
    public static void SetDragSensitivity(float value)
    {
        DragSensitivity = value;
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = value;
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = value;
    }

    public static void SetSFXVolume(float value)
    {
        SFXVolume = value;
    }

    public static void SetMute(bool isMuted)
    {
        IsMuted = isMuted;
    }
}