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

    [Header("Gameplay HUD")]
    [Tooltip("Player HUD canvas/root to hide while paused so it doesn't overlay the pause menu.")]
    public GameObject playerHUD;

    private SettingsMenu _settingsMenu;

    void Start()
    {
        _settingsMenu = GetComponentInChildren<SettingsMenu>(true);

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (playerHUD != null) playerHUD.SetActive(true);
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (playerHUD != null) playerHUD.SetActive(false);
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
        
        if (_settingsMenu != null)
        {
            _settingsMenu.LoadAndApplySettings();
        }
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
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}