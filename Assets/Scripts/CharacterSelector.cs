using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    // =========================
    // CHARACTER SYSTEM
    // =========================

    public GameObject[] characters;

    private int currentCharacterIndex = 0;

    // =========================
    // CO-OP TWO-STEP SELECTION
    // =========================

    [Header("Co-op Selection")]
    [Tooltip("Optional label that shows whose turn it is to choose (e.g. 'Player 1 - Choose your character').")]
    public TMP_Text turnLabel;

    // Which player is currently choosing in co-op (1 or 2).
    private int currentPlayerTurn = 1;

    // PlayerPrefs keys
    private const string KeyP1 = "Player1Character";
    private const string KeyP2 = "Player2Character";
    private const string KeyLegacy = "SelectedCharacter";

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

        currentPlayerTurn = 1;
        UpdateTurnLabel();
    }

    // =========================
    // TURN LABEL
    // =========================

    private void UpdateTurnLabel()
    {
        if (turnLabel == null) return;

        // Only meaningful in co-op; single player skips this scene entirely.
        string mode = PlayerPrefs.GetString("GameMode", "Multiplayer");
        if (mode == "SinglePlayer")
            turnLabel.text = "Choose your character";
        else
            turnLabel.text = "Player " + currentPlayerTurn + " - Choose your character";
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
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        string gameMode = PlayerPrefs.GetString("GameMode", "Multiplayer");

        // SINGLE PLAYER (legacy safety): this scene is normally skipped for single
        // player, but if it is reached, behave like the old single-character flow.
        if (gameMode == "SinglePlayer")
        {
            PlayerPrefs.SetInt(KeyLegacy, currentCharacterIndex);
            PlayerPrefs.Save();
            Invoke(nameof(LoadGameScene), 0.7f);
            return;
        }

        // CO-OP: first Player 1 chooses, then Player 2 chooses, then load.
        if (currentPlayerTurn == 1)
        {
            PlayerPrefs.SetInt(KeyP1, currentCharacterIndex);
            PlayerPrefs.Save();
            Debug.Log("[CharacterSelector] Player 1 chose index " + currentCharacterIndex);

            // Hand over to Player 2: reset the carousel to the first character.
            currentPlayerTurn = 2;
            ResetSelectionForNextPlayer();
            UpdateTurnLabel();
        }
        else
        {
            PlayerPrefs.SetInt(KeyP2, currentCharacterIndex);
            PlayerPrefs.Save();
            Debug.Log("[CharacterSelector] Player 2 chose index " + currentCharacterIndex);

            Invoke(nameof(LoadGameScene), 0.7f);
        }
    }

    // =========================
    // RESET CAROUSEL FOR NEXT PLAYER
    // =========================

    private void ResetSelectionForNextPlayer()
    {
        // Hide the character Player 1 left visible.
        if (characters.Length > 0 && characters[currentCharacterIndex] != null)
            characters[currentCharacterIndex].SetActive(false);

        currentCharacterIndex = 0;

        if (characters.Length > 0 && characters[currentCharacterIndex] != null)
            characters[currentCharacterIndex].SetActive(true);
    }

    // =========================
    // LOAD GAME SCENE
    // =========================

    void LoadGameScene()
    {
        // Load the co-op or single-player variant depending on the mode the
        // player picked in the main menu (defaults to Multiplayer if unset).
        string gameMode = PlayerPrefs.GetString("GameMode", "Multiplayer");

        if (gameMode == "SinglePlayer")
            SceneManager.LoadScene("HM_Demo_SinglePlayer");
        else
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