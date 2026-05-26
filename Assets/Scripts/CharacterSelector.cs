using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    public GameObject[] characters;
    private int currentCharacterIndex = 0;

    void Start()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(false);
        }
        
        if (characters.Length > 0)
        {
            characters[currentCharacterIndex].SetActive(true);
        }
    }

    public void NextCharacter()
    {
        characters[currentCharacterIndex].SetActive(false);
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;
        characters[currentCharacterIndex].SetActive(true);
    }

    public void PreviousCharacter()
    {
        characters[currentCharacterIndex].SetActive(false);
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characters.Length - 1;
        }
        characters[currentCharacterIndex].SetActive(true);
    }

    public void PlayGame()
    {
        // 1. Save the chosen character
        PlayerPrefs.SetInt("SelectedCharacter", currentCharacterIndex);
        
        // 2. Check which mode was clicked in the Main Menu
        string gameMode = PlayerPrefs.GetString("GameMode", "SinglePlayer");

        // 3. Send the player to the correct gameplay scene
        if (gameMode == "Multiplayer")
        {
            SceneManager.LoadScene("MultiplayerGameplay");
        }
        else
        {
            SceneManager.LoadScene("SinglePlayerGameplay");
        }
    }

    public void BackToMenu()
    {
        // Takes you safely back to your main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}