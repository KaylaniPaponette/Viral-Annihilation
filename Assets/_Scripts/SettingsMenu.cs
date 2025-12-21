// ===== SettingsMenu.cs (SIMPLIFIED) =====
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;

    [Header("Audio")]
    public AudioMixer audioMixer; // Still needs its own reference for real-time changes

    // This method is called from UIManager or MainMenuManager to set up the UI
    public void RefreshUI()
    {
        // Update the UI elements to reflect the current loaded settings
        masterSlider.value = SettingsManager.MasterVolume;
        musicSlider.value = SettingsManager.MusicVolume;
        sfxSlider.value = SettingsManager.SFXVolume;
        muteToggle.isOn = SettingsManager.IsMuted;
        sensitivitySlider.value = SettingsManager.DragSensitivity / 0.5f;
        // Display the percentage based on the slider (0.0 to 1.0)
        sensitivityValueText.text = (sensitivitySlider.value * 100).ToString("F0") + "%";
        //sensitivityValueText.text = (SettingsManager.DragSensitivity * 100).ToString("F0") + "%";
    }

    // The methods below are called by the UI elements' OnValueChanged events

    public void OnMasterVolumeChanged(float volume)
    {
        SettingsManager.SetMasterVolume(volume);
        SettingsManager.ApplyAudioSettings(audioMixer);
        SettingsManager.SaveSettings();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        SettingsManager.SetMusicVolume(volume);
        SettingsManager.ApplyAudioSettings(audioMixer);
        SettingsManager.SaveSettings();
    }

    public void OnSFXVolumeChanged(float volume)
    {
        SettingsManager.SetSFXVolume(volume);
        SettingsManager.ApplyAudioSettings(audioMixer);
        SettingsManager.SaveSettings();
    }

    public void OnMuteToggled(bool isMuted)
    {
        SettingsManager.SetMute(isMuted);
        SettingsManager.ApplyAudioSettings(audioMixer);
        SettingsManager.SaveSettings();
    }

    public void OnSensitivityChanged(float value)
    {
        // Remap the 0.0-1.0 slider value to a 0.0-0.5 actual sensitivity value
        float actualSensitivity = value * 0.5f;

        SettingsManager.SetDragSensitivity(actualSensitivity);

        // Update the text to show the slider's percentage (0-100%)
        sensitivityValueText.text = (value * 100).ToString("F0") + "%";
        SettingsManager.SaveSettings();
    }
    //public void OnSensitivityChanged(float value)
    //{
    //    SettingsManager.SetDragSensitivity(value);
    //    sensitivityValueText.text = (value * 100).ToString("F0") + "%";
    //    SettingsManager.SaveSettings();
    //}
}