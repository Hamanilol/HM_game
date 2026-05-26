using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    // Changed these to target your character selection scene first
    [SerializeField] private string characterSelectScene = "CharacterSelect"; 
    [SerializeField] private string optionsScene = "OptionsMenu";
    [SerializeField] private string creditsScene = "CreditsMenu";

    [Header("Audio Setup")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }
    }

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

    // Both singleplayer and multiplayer buttons now open Character Selection
    // but save the intended game mode first!
    public void PlaySinglePlayer()
    {
        PlayClickSound();
        PlayerPrefs.SetString("GameMode", "SinglePlayer");
        SceneManager.LoadScene(characterSelectScene);
    }

    public void PlayMultiplayer()
    {
        PlayClickSound();
        PlayerPrefs.SetString("GameMode", "Multiplayer");
        SceneManager.LoadScene(characterSelectScene);
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