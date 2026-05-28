using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Abdulrahman.WaveSystem;

namespace Abdulrahman.WaveSystem
{
    // Handles all the on-screen wave info.
    public class WaveUIManager : MonoBehaviour
    {
        [Header("REFERENCES")]
        // AdulRahman EnemySpawner GameObject in here
        public EnemySpawner spawner;

        [Header("UI ELEMENTS")]
        // small corner text showing the current wave name
        public TextMeshProUGUI waveNameText;

        // live counter showing how many enemies are still alive
        public TextMeshProUGUI enemyCountText;

        // big centered text that briefly appears when a wave starts or ends
        public TextMeshProUGUI waveBannerText;

        [Header("BANNER SETTINGS")]
        // how long the banner stays up before fading out
        public float bannerDuration = 3f;

        private void OnEnable()
        {
            // try to find the spawner automatically if nothing was dragged in
            if (spawner == null)
            {
                spawner = FindObjectOfType<EnemySpawner>();
                if (spawner == null)
                {
                    Debug.LogWarning("[WaveUIManager] Couldn't find an EnemySpawner in the scene.");
                    return;
                }
            }

            spawner.OnWaveStarted       += HandleWaveStarted;
            spawner.OnWaveCompleted     += HandleWaveCompleted;
            spawner.OnAllWavesCompleted += HandleAllWavesCompleted;
        }

        private void OnDisable()
        {
            // always clean up event subscriptions when the object is disabled
            if (spawner == null) return;
            spawner.OnWaveStarted       -= HandleWaveStarted;
            spawner.OnWaveCompleted     -= HandleWaveCompleted;
            spawner.OnAllWavesCompleted -= HandleAllWavesCompleted;
        }

        private void Update()
        {
            // keep the corner labels updated every frame
            if (spawner == null) return;

            if (waveNameText != null && spawner.CurrentWave != null)
                waveNameText.text = spawner.CurrentWave.waveName;

            if (enemyCountText != null)
                enemyCountText.text = $"Enemies left: {spawner.EnemiesAlive}";
        }

        private void HandleWaveStarted(int index, WaveConfig cfg)
        {
            ShowBanner($"⚠  {cfg.waveName}  ⚠\n{cfg.totalEnemies} enemies incoming!");
        }

        private void HandleWaveCompleted(int index)
        {
            // don't show "wave cleared" after the very last wave the all-complete handler takes care of that
            bool isLastWave = (index >= spawner.waves.Length - 1);
            if (!isLastWave)
                ShowBanner($"Wave {index + 1} cleared!\nGet ready...");
        }

        private void HandleAllWavesCompleted()
        {
            ShowBanner("★  YOU SURVIVED  ★\nAll waves defeated!", permanent: true);

            // hide the enemy counter since there's nothing left to count
            if (enemyCountText != null)
                enemyCountText.text = "";
        }

        private Coroutine _bannerCoroutine;

        private void ShowBanner(string message, bool permanent = false)
        {
            if (waveBannerText == null) return;

            // cancel any banner that's currently showing before we put up a new one
            if (_bannerCoroutine != null)
                StopCoroutine(_bannerCoroutine);

            _bannerCoroutine = StartCoroutine(BannerRoutine(message, permanent));
        }

        private IEnumerator BannerRoutine(string message, bool permanent)
        {
            waveBannerText.text    = message;
            waveBannerText.enabled = true;

            if (!permanent)
            {
                yield return new WaitForSeconds(bannerDuration);
                waveBannerText.enabled = false;
            }
        }
    }
}
