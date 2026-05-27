using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    // =========================
    // CHARACTER SYSTEM
    // =========================

    public GameObject[] characters;

    private int currentCharacterIndex = 0;

    // =========================
    // AUDIO SYSTEM
    // =========================

    public AudioSource audioSource;

    public AudioClip clickSound;
    public AudioClip backSound;

    // =========================
    // START
    // =========================

    void Start()
    {
        // TURN OFF ALL CHARACTERS
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(false);
        }

        // SHOW FIRST CHARACTER
        if (characters.Length > 0)
        {
            characters[currentCharacterIndex].SetActive(true);
        }
    }

    // =========================
    // NEXT CHARACTER
    // =========================

    public void NextCharacter()
    {
        // PLAY SOUND
        audioSource.PlayOneShot(clickSound);

        // HIDE CURRENT
        characters[currentCharacterIndex].SetActive(false);

        // MOVE TO NEXT
        currentCharacterIndex++;

        // LOOP BACK
        if (currentCharacterIndex >= characters.Length)
        {
            currentCharacterIndex = 0;
        }

        // SHOW NEW CHARACTER
        characters[currentCharacterIndex].SetActive(true);
    }

    // =========================
    // PREVIOUS CHARACTER
    // =========================

    public void PreviousCharacter()
    {
        // PLAY SOUND
        audioSource.PlayOneShot(clickSound);

        // HIDE CURRENT
        characters[currentCharacterIndex].SetActive(false);

        // MOVE BACK
        currentCharacterIndex--;

        // LOOP TO END
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characters.Length - 1;
        }

        // SHOW NEW CHARACTER
        characters[currentCharacterIndex].SetActive(true);
    }

    // =========================
    // PLAY GAME BUTTON
    // =========================

    public void PlayGame()
    {
        // PLAY CLICK SOUND
        audioSource.PlayOneShot(clickSound);

        // SAVE CHARACTER
        PlayerPrefs.SetInt("SelectedCharacter", currentCharacterIndex);

        // WAIT BEFORE LOADING
        Invoke(nameof(LoadGameScene), 1f);
    }

    // =========================
    // LOAD GAME SCENE
    // =========================

    void LoadGameScene()
    {
        // GET GAME MODE
        string gameMode = PlayerPrefs.GetString("GameMode", "SinglePlayer");

        // LOAD CORRECT SCENE
        if (gameMode == "Multiplayer")
        {
            SceneManager.LoadScene("MultiplayerGameplay");
        }
        else
        {
            SceneManager.LoadScene("SinglePlayerGameplay");
        }
    }

    // =========================
    // BACK BUTTON
    // =========================

    public void BackToMenu()
    {
        // PLAY BACK SOUND
        audioSource.PlayOneShot(backSound);

        // WAIT BEFORE LOADING MENU
        Invoke(nameof(LoadMenuScene), 0.3f);
    }

    // =========================
    // LOAD MENU
    // =========================

    void LoadMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}