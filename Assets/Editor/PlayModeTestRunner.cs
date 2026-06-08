using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Abdulrahman.EnemySystem;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";

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
                    if (EditorApplication.isPlaying)
                    {
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying) EditorApplication.update += WaitFramesThenRun;
                    break;
                case "Done":
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetString(StateKey, "InPlayMode");
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static double _testStartTime = 0;
        private static bool _spawnedAnyProjectile = false;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                Setup();
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool complete = Tick(elapsed);

            if (complete || elapsed >= TestTimeout)
            {
                FinishTest();
            }
        }

        private static void Setup()
        {
            var priest = Object.FindAnyObjectByType<PriestAI>();
            if (priest != null)
            {
                priest.rangedRange = 100f;
                priest.attackCooldown = 1f;
                Debug.Log("[Test] Priest found: " + priest.name);
            }
            var player = GameObject.FindWithTag("Player");
            if (player != null && priest != null)
            {
                player.transform.position = priest.transform.position + Vector3.forward * 10f;
            }
        }

        private static bool Tick(float elapsed)
        {
            var projectiles = Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
            if (projectiles.Length > 0)
            {
                _spawnedAnyProjectile = true;
                foreach (var p in projectiles)
                {
                    Debug.Log("[Test] Projectile Detected: " + p.name + " at " + p.transform.position);
                }
            }
            return _spawnedAnyProjectile && elapsed > 5f;
        }

        private static void FinishTest()
        {
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;
            SessionState.SetString(ResultKey, GetResult());
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
            public bool spawned;
            public string[] logs;
        }

        private static string GetResult()
        {
            return JsonUtility.ToJson(new TestResult
            {
                spawned = _spawnedAnyProjectile,
                logs = _capturedLogs.ToArray()
            });
        }
    }
}