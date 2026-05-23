using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string singlePlayerScene = "SinglePlayerGameplay";
    [SerializeField] private string multiplayerScene = "MultiplayerGameplay";
    [SerializeField] private string optionsScene = "OptionsMenu";
    [SerializeField] private string creditsScene = "CreditsMenu";

    [Header("Audio Setup")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        // Ensures background music starts playing immediately on menu load
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }
    }

    // Public audio functions to be called by buttons
    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    // Scene Navigation functions
    public void PlaySinglePlayer()
    {
        PlayClickSound();
        SceneManager.LoadScene(singlePlayerScene);
    }

    public void PlayMultiplayer()
    {
        PlayClickSound();
        SceneManager.LoadScene(multiplayerScene);
    }

    public void OpenOptions()
    {
        PlayClickSound();
        SceneManager.LoadScene(optionsScene);
    }

    public void OpenCredits()
    {
        PlayClickSound();
        SceneManager.LoadScene(creditsScene);
    }

    public void QuitGame()
    {
        PlayClickSound();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}