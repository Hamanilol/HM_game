using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]

    [SerializeField] private string characterSelectScene = "CharacterSelect";

    [Header("Settings Menu")]

    [SerializeField] private GameObject settingsMenu;

    [Header("Credits Video")]

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject mainMenu;

    [Header("Audio Setup")]

    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    // =========================
    // START
    // =========================

    private void Start()
    {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }
    }

    // =========================
    // HOVER SOUND
    // =========================

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    // =========================
    // CLICK SOUND
    // =========================

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    // =========================
    // SINGLEPLAYER
    // =========================

    public void PlaySinglePlayer()
    {
        // PLAY SOUND
        PlayClickSound();

        // SAVE MODE
        PlayerPrefs.SetString("GameMode", "SinglePlayer");

        // DELAY LOAD
        Invoke(nameof(LoadSinglePlayerScene), 0.7f);
    }

    void LoadSinglePlayerScene()
    {
        SceneManager.LoadScene(characterSelectScene);
    }

    // =========================
    // MULTIPLAYER
    // =========================

    public void PlayMultiplayer()
    {
        // PLAY SOUND
        PlayClickSound();

        // SAVE MODE
        PlayerPrefs.SetString("GameMode", "Multiplayer");

        // DELAY LOAD
        Invoke(nameof(LoadMultiplayerScene), 0.7f);
    }

    void LoadMultiplayerScene()
    {
        SceneManager.LoadScene(characterSelectScene);
    }

    // =========================
    // OPTIONS
    // =========================

    public void OpenOptions()
    {
        // PLAY SOUND
        PlayClickSound();

        // DELAY OPEN
        Invoke(nameof(ShowSettingsUI), 0.7f);
    }

    void ShowSettingsUI()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
            mainMenu.SetActive(false);
        }
    }

    public void CloseOptions()
    {
        // PLAY SOUND
        PlayClickSound();

        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
    }

    // =========================
    // CREDITS
    // =========================

    public void OpenCredits()
    {
        // PLAY SOUND
        PlayClickSound();

        // DELAY VIDEO
        Invoke(nameof(PlayCreditsVideo), 0.7f);
    }

    void PlayCreditsVideo()
    {
        // HIDE MENU
        mainMenu.SetActive(false);

        // PLAY VIDEO
        videoPlayer.Play();

        // WAIT FOR VIDEO TO END
        videoPlayer.loopPointReached += EndCreditsVideo;
    }

    void EndCreditsVideo(VideoPlayer vp)
    {
        // STOP VIDEO
        videoPlayer.Stop();

        // SHOW MENU AGAIN
        mainMenu.SetActive(true);
    }

    // =========================
    // QUIT GAME
    // =========================

    public void QuitGame()
    {
        // PLAY SOUND
        PlayClickSound();

        // DELAY QUIT
        Invoke(nameof(QuitAfterSound), 0.7f);
    }

    void QuitAfterSound()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}