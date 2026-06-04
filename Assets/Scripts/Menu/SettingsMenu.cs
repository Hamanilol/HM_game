using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Settings UI Elements")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    private void Start()
    {
        LoadAndApplySettings();
    }

    public void LoadAndApplySettings()
    {
        // Sensitivity
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        }

        // Volume
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }

        // Quality
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            string[] qualityNames = QualitySettings.names;
            List<string> options = new List<string>(qualityNames);
            qualityDropdown.AddOptions(options);

            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            qualityDropdown.value = savedQuality;
            qualityDropdown.RefreshShownValue();
            SetQuality(savedQuality);
        }

        // Fullscreen
        if (fullscreenToggle != null)
        {
            int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1);
            bool isFullscreen = (savedFullscreen == 1);
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Convert to logarithmic scale
            float db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20;
            audioMixer.SetFloat("MasterVolume", db);
        }

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
