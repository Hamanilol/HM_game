using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public void RestartGame()
    {
        // Reset time scale in case it was paused
        Time.timeScale = 1f;
        
        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Reset time scale in case it was paused
        Time.timeScale = 1f;
        
        // Load the MainMenu scene
        SceneManager.LoadScene("MainMenu");
    }
}