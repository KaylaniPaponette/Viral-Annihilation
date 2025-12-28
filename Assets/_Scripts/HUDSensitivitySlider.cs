using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDSensitivitySlider : MonoBehaviour
{
    private Slider sensitivitySlider;

    [Header("UI References")]
    [Tooltip("The text element that will show the percentage (e.g., 50%)")]
    public TextMeshProUGUI sensitivityValueText;

    void Awake()
    {
        sensitivitySlider = GetComponent<Slider>();
    }

    // OnEnable runs every time the HUD is shown or the scene starts
    void OnEnable()
    {
        RefreshSliderPosition();
    }

    public void RefreshSliderPosition()
    {
        if (sensitivitySlider != null)
        {
            // Pull the shared value from SettingsManager
            // We map the 0.0-0.5 sensitivity back to the 0.0-1.0 slider range
            float currentSettingsValue = SettingsManager.DragSensitivity / 0.5f;

            // Set the slider position without triggering the 'onValueChanged' event 
            // to avoid redundant saving during initialization
            sensitivitySlider.SetValueWithoutNotify(currentSettingsValue);

            UpdateText(currentSettingsValue);
        }
    }

    void Start()
    {
        if (sensitivitySlider != null)
        {
            // This ensures that moving this slider calls the logic below
            sensitivitySlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // 1. Remap 0.0-1.0 slider to 0.0-0.5 actual sensitivity
        float actualSensitivity = value * 0.5f;

        // 2. Update the shared static property in SettingsManager
        // This is the SAME method called by SettingsMenu.cs
        SettingsManager.SetDragSensitivity(actualSensitivity);

        // 3. Update the HUD text
        UpdateText(value);

        // 4. Save to PlayerPrefs so it is remembered
        SettingsManager.SaveSettings();
    }

    private void UpdateText(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = (value * 100).ToString("F0") + "%";
        }
    }
}