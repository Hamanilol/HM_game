using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string singlePlayerScene = "SinglePlayerGameplay";
    [SerializeField] private string multiplayerScene = "MultiplayerGameplay";
    [SerializeField] private string optionsScene = "OptionsMenu";
    [SerializeField] private string creditsScene = "CreditsMenu"; // New credits scene variable

    public void PlaySinglePlayer()
    {
        SceneManager.LoadScene(singlePlayerScene);
    }

    public void PlayMultiplayer()
    {
        SceneManager.LoadScene(multiplayerScene);
    }

    public void OpenOptions()
    {
        SceneManager.LoadScene(optionsScene);
    }

    public void OpenCredits()
    {
        // Loads the credits scene when the button is clicked
        SceneManager.LoadScene(creditsScene);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}