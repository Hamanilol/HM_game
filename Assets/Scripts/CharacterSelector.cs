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
        Debug.Log("[CharacterSelector] Start. Character count: " + characters.Length);
        // TURN OFF ALL CHARACTERS
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                characters[i].SetActive(false);
            }
        }

        // SHOW FIRST CHARACTER
        if (characters.Length > 0)
        {
            characters[currentCharacterIndex].SetActive(true);
            Debug.Log("[CharacterSelector] Initialized with: " + characters[currentCharacterIndex].name);
        }
    }

    // =========================
    // NEXT CHARACTER
    // =========================

    public void NextCharacter()
    {
        // PLAY SOUND
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // HIDE CURRENT
        if (characters[currentCharacterIndex] != null)
            characters[currentCharacterIndex].SetActive(false);

        // MOVE TO NEXT
        currentCharacterIndex++;

        // LOOP BACK
        if (currentCharacterIndex >= characters.Length)
        {
            currentCharacterIndex = 0;
        }

        // SHOW NEW CHARACTER
        if (characters[currentCharacterIndex] != null)
        {
            characters[currentCharacterIndex].SetActive(true);
            Debug.Log("[CharacterSelector] Switched to Next: " + characters[currentCharacterIndex].name + " (Index " + currentCharacterIndex + ")");
        }
    }

    // =========================
    // PREVIOUS CHARACTER
    // =========================

    public void PreviousCharacter()
    {
        // PLAY SOUND
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // HIDE CURRENT
        if (characters[currentCharacterIndex] != null)
            characters[currentCharacterIndex].SetActive(false);

        // MOVE BACK
        currentCharacterIndex--;

        // LOOP TO END
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characters.Length - 1;
        }

        // SHOW NEW CHARACTER
        if (characters[currentCharacterIndex] != null)
        {
            characters[currentCharacterIndex].SetActive(true);
            Debug.Log("[CharacterSelector] Switched to Previous: " + characters[currentCharacterIndex].name + " (Index " + currentCharacterIndex + ")");
        }
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
        PlayerPrefs.Save();

        // WAIT BEFORE LOADING
        Invoke(nameof(LoadGameScene), 0.7f);
}

    // =========================
    // LOAD GAME SCENE
    // =========================

    void LoadGameScene()
    {
        // LOAD DEMO SCENE
        SceneManager.LoadScene("HM_Demo");
    }

    // =========================
    // BACK BUTTON
    // =========================

    public void BackToMenu()
    {
        // PLAY BACK SOUND
        audioSource.PlayOneShot(backSound);

        // WAIT BEFORE LOADING MENU
        Invoke(nameof(LoadMenuScene), 0.7f);
    }

    // =========================
    // LOAD MENU
    // =========================

    void LoadMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}