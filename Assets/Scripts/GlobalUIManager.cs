using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalUIManager : MonoBehaviour
{
    [SerializeField] private string currencySceneName = "CurrencyUI";

    private void Awake()
    {
        LoadCurrencyUI();
    }

    private void LoadCurrencyUI()
    {
        // Check if scene is already loaded
        bool isLoaded = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == currencySceneName)
            {
                isLoaded = true;
                break;
            }
        }

        if (!isLoaded)
        {
            SceneManager.LoadScene(currencySceneName, LoadSceneMode.Additive);
        }
    }
}
