using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]

    [SerializeField] private string characterSelectScene = "CharacterSelect";
    [SerializeField] private string optionsScene = "OptionsMenu";
    [SerializeField] private string creditsScene = "CreditsMenu";

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
        Invoke(nameof(LoadSinglePlayerScene), 1f);
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
        Invoke(nameof(LoadMultiplayerScene), 0.3f);
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

        // DELAY LOAD
        Invoke(nameof(LoadOptionsScene), 0.3f);
    }

    void LoadOptionsScene()
    {
        SceneManager.LoadScene(optionsScene);
    }

    // =========================
    // CREDITS
    // =========================

    public void OpenCredits()
    {
        // PLAY SOUND
        PlayClickSound();

        // DELAY LOAD
        Invoke(nameof(LoadCreditsScene), 0.3f);
    }

    void LoadCreditsScene()
    {
        SceneManager.LoadScene(creditsScene);
    }

    // =========================
    // QUIT GAME
    // =========================

    public void QuitGame()
    {
        // PLAY SOUND
        PlayClickSound();

        // DELAY QUIT
        Invoke(nameof(QuitAfterSound), 0.3f);
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