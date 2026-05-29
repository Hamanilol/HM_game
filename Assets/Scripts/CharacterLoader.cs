using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    public class CharacterLoader : MonoBehaviour
    {
        public GameObject[] characters;
        public string prefKey = "SelectedCharacter";

        private void Awake()
        {
            int selectedIndex = PlayerPrefs.GetInt(prefKey, 0);
            Debug.Log("[CharacterLoader] Selected Index from PlayerPrefs: " + selectedIndex);

            if (characters == null || characters.Length == 0)
            {
                Debug.LogWarning("[CharacterLoader] No characters assigned to the loader!");
                return;
            }

            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null)
                {
                    bool shouldBeActive = (i == selectedIndex);
                    characters[i].SetActive(shouldBeActive);
                    Debug.Log("[CharacterLoader] Character " + characters[i].name + " (Index " + i + ") set active: " + shouldBeActive);
                    
                    if (shouldBeActive)
                    {
                        characters[i].tag = "Player";
                    }
                    else
                    {
                        characters[i].tag = "Untagged";
                    }
                }
                else
                {
                    Debug.LogWarning("[CharacterLoader] Character at index " + i + " is null!");
                }
            }
        }
    }
}
