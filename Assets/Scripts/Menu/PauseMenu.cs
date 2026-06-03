using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic; 
public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    [Header("Settings")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    void Start()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // When the menu first loads, make sure the slider matches the saved setting
        // If there is no saved setting yet, it defaults to 2f
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        }

        // Load saved volume
        if (volumeSlider != null)
        {
            // Default to 1 (max volume) if they haven't saved a setting yet
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }

        if (qualityDropdown != null)
        {
            // 1. Wipe out the default "Option A, Option B" list
            qualityDropdown.ClearOptions();

            // 2. Ask Unity for the names of the graphics levels in your project
            string[] qualityNames = QualitySettings.names;

            // 3. Convert the array into a List, and feed it to the dropdown
            List<string> options = new List<string>(qualityNames);
            qualityDropdown.AddOptions(options);

            // 4. Load the saved preference (or default to whatever Unity is currently using)
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            qualityDropdown.value = savedQuality;
            qualityDropdown.RefreshShownValue();

            // 5. Apply the setting just to be safe
            SetQuality(savedQuality);
        }

        if (fullscreenToggle != null)
        {
            // Get the saved number (default to 1, which means True)
            int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1);

            // Translate the number back into a true/false statement
            bool isFullscreen = (savedFullscreen == 1);

            // Update the UI checkmark
            fullscreenToggle.isOn = isFullscreen;

            // Apply the setting to the screen
            Screen.fullScreen = isFullscreen;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the settings menu is currently open, go back to the pause menu
            if (settingsMenuUI.activeSelf)
            {
                BackToPause();
            }
            // Otherwise, toggle pause/resume normally
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false); // Ensure this is closed too
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- NEW SETTINGS LOGIC ---

    public void LoadSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void BackToPause()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // --------------------------

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    public void SetSensitivity(float sensitivity)
    {
        // Save the new value to the PlayerPrefs notepad
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        // Convert the 0.0001 to 1 slider value into a logarithmic decibel scale
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);

        // Save it to the digital notepad
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetQuality(int qualityIndex)
    {
        // Tell Unity's engine to switch the graphics level
        QualitySettings.SetQualityLevel(qualityIndex);

        // Save the choice to the digital notepad
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // Tell Unity to change the window mode
        Screen.fullScreen = isFullscreen;

        // Translate the true/false into a 1 or 0 for the filing clerk
        int boolAsNumber = isFullscreen ? 1 : 0;

        // Save it to the digital notepad
        PlayerPrefs.SetInt("IsFullscreen", boolAsNumber);
        PlayerPrefs.Save();
    }
}