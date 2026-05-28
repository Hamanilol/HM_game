using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Abdulrahman.EnemySystem;

namespace Abdulrahman.UISystem
{
    public class BossHealthBar : MonoBehaviour
    {
        [Header("UI REFERENCES")]
        public GameObject rootPanel;
        public Slider healthBar;
        public TextMeshProUGUI bossNameText;
        public Image fillImage;

        [Header("SETTINGS")]
        public Color healthyColor = Color.red;
        public Color criticalColor = new Color(0.5f, 0, 0);

        private EnemyHealth _currentBoss;

        private void Start()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        public void InitializeBoss(EnemyHealth boss, string bossName)
        {
            _currentBoss = boss;
            if (bossNameText != null) bossNameText.text = bossName;
            
            if (rootPanel != null) rootPanel.SetActive(true);
            
            boss.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(boss._currentHealth, boss.maxHealth);
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.value = current / max;
            }

            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(criticalColor, healthyColor, current / max);
            }

            if (current <= 0)
            {
                if (_currentBoss != null) _currentBoss.OnHealthChanged -= UpdateHealthBar;
                Invoke(nameof(HideBar), 2f);
            }
        }

        private void HideBar()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
        }
    }
}
