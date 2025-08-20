using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;
    [Header("Drag Sensitivity")]
    public Slider sensitivitySlider;
    public TMPro.TextMeshProUGUI sensitivityValueText; // To display the slider's value

    [Header("Audio")]
    public AudioMixer audioMixer;

    void Start()
    {
        // Hide the panel at the start
        settingsPanel.SetActive(false);

        // Load saved settings and apply them
        LoadSettings();
    }

    public void SetMasterVolume(float volume)
    {
        // THE FIX: Check for 0 to prevent a math error.
        float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MasterVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {

        // THE FIX: Check for 0 to prevent a math error.
        float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MusicVolume", dbVolume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        // THE FIX: Check for 0 to prevent a math error.
        float dbVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void ToggleMute(bool isMuted)
    {
        if (isMuted)
        {
            // Mute by setting volume to the lowest possible value
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        else
        {
            // Unmute by restoring the volume from the slider's current value
            SetMasterVolume(masterSlider.value);
        }
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
    }

    public void OnSensitivitySliderChanged(float value)
    {
        // When the slider moves, save the new value using the simplified manager
        SettingsManager.SaveSettings(value);

        // Update the text to show the current value (e.g., "50%")
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = (value * 100).ToString("F0") + "%";
        }
    }
    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    private void LoadSettings()
    {
        // Load slider values, defaulting to 1 (max volume) if no value is saved
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Load mute state, defaulting to not muted (0)
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        muteToggle.isOn = isMuted;

        // Apply the loaded settings immediately
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);

        // Apply mute setting last, as it overrides master volume
        if (isMuted)
        {
            audioMixer.SetFloat("MasterVolume", -80f);
        }

        // --- NEW SIMPLIFIED SENSITIVITY LOADING ---
        // Load drag sensitivity settings from our manager
        sensitivitySlider.value = SettingsManager.DragSensitivity;

        // THE FIX: This ensures the text is updated as soon as the settings menu is opened.
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = (sensitivitySlider.value * 100).ToString("F0") + "%";
        }
    }

}