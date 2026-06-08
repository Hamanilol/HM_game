using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Abdulrahman.PlayerSystem;
using Abdulrahman.EnemySystem;
using Abdulrahman.InventorySystem;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 10);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 15.0f);

        private static List<string> _capturedLogs = new List<string>();

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "WaitingForCompile":
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying) EditorApplication.update += WaitFramesThenRun;
                    break;
                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                Setup();
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;
            bool complete = Tick(elapsed);

            if (complete || timedOut)
            {
                FinishTest(timedOut && !complete, timedOut ? "Test timed out" : null);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;
            string resultJson = GetResult();
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            _capturedLogs.Add("[" + type + "] " + message);
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
                AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
        }

        private static QuickSwapInventory _inv;

        private static void Setup()
        {
            var player = GameObject.Find("Female1");
            if (player != null)
            {
                _inv = player.GetComponent<QuickSwapInventory>();
                var ghoul = GameObject.Find("Ghoul");
                if (ghoul != null)
                {
                    // Move player to look at ghoul
                    player.transform.position = ghoul.transform.position + Vector3.forward * 5f;
                    player.transform.LookAt(ghoul.transform.position + Vector3.up * 1f);
                    
                    var cam = player.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        cam.transform.LookAt(ghoul.transform.position + Vector3.up * 1f);
                    }
                }
            }
        }

        private static int _shotCount = 0;
        private static bool Tick(float elapsed)
        {
            if (_inv == null) return true;
            
            if (_shotCount < 5 && elapsed > 1.0f + _shotCount * 0.5f)
            {
                var weapon = _inv.GetCurrentWeapon();
                if (weapon != null)
                {
                    Debug.Log("[Test] Simulating TryFire");
                    weapon.TryFire();
                    _shotCount++;
                }
            }

            return _shotCount >= 5 || elapsed > 10.0f;
        }

        private static string GetResult()
        {
            return JsonUtility.ToJson(new TestResult
            {
                success = true,
                logs = _capturedLogs.ToArray()
            });
        }
    }
}