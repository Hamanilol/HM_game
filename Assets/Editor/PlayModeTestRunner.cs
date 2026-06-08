
using UnityEngine;
using UnityEditor;
using System.Collections;
using Abdulrahman.InventorySystem;

[InitializeOnLoad]
public static class PlayModeTestRunner
{
    static PlayModeTestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        // If we are already in play mode and state is waiting, run it
        if (EditorApplication.isPlaying && SessionState.GetString("PlayModeTest.State", "Idle") == "EnteringPlayMode")
        {
            RunTest();
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (SessionState.GetString("PlayModeTest.State", "Idle") == "EnteringPlayMode")
            {
                RunTest();
            }
        }
    }

    private static void RunTest()
    {
        SessionState.SetString("PlayModeTest.State", "InPlayMode");
        var testRunner = new GameObject("TestRunner").AddComponent<TestRunnerBehaviour>();
    }

    private class TestRunnerBehaviour : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f); // Wait for initialization

            GameObject player = GameObject.Find("Female1");
            if (player == null)
            {
                Finish("ERROR: Female1 not found");
                yield break;
            }

            QuickSwapInventory inv = player.GetComponent<QuickSwapInventory>();
            if (inv == null)
            {
                Finish("ERROR: QuickSwapInventory not found on Female1");
                yield break;
            }

            BaseWeapon weapon = inv.GetCurrentWeapon();
            if (weapon == null)
            {
                // Try waiting another second
                yield return new WaitForSeconds(1f);
                weapon = inv.GetCurrentWeapon();
            }

            if (weapon == null)
            {
                Finish("ERROR: No weapon equipped on Female1");
                yield break;
            }

            Debug.Log("[Test] Firing weapon: " + weapon.name);
            weapon.TryFire();

            yield return null; // Wait for PlayClipAtPoint to create the object

            AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            bool foundSound = false;
            foreach (var s in sources)
            {
                if (s.clip != null && s.clip.name.Contains("mixkit-game-gun-shot"))
                {
                    foundSound = true;
                    Debug.Log("[Test] Found AudioSource with gunshot sound: " + s.clip.name);
                    break;
                }
            }

            if (foundSound)
            {
                Finish("SUCCESS: Gunshot sound played correctly");
            }
            else
            {
                Finish("FAILURE: Gunshot sound NOT found after firing");
            }
        }

        private void Finish(string result)
        {
            SessionState.SetString("PlayModeTest.Result", result);
            SessionState.SetString("PlayModeTest.State", "Done");
            Debug.Log("[Test] " + result);
            EditorApplication.isPlaying = false;
            Destroy(gameObject);
        }
    }
}
