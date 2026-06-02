using UnityEngine;
using UnityEngine.UI;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.PlayerSystem
{
    public class HealthUI : MonoBehaviour
    {
        [Header("References")]
        public PlayerHealth playerHealth;
        public Slider healthSlider;
        public Image fillImage;

        [Header("Settings")]
        public Color highHealthColor = Color.green;
        public Color midHealthColor = Color.yellow;
        public Color lowHealthColor = Color.red;

        private void OnEnable()
        {
            if (playerHealth == null)
                playerHealth = Object.FindAnyObjectByType<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
            }
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (fillImage != null)
            {
                float percentage = current / max;
                if (percentage > 0.5f)
                    fillImage.color = Color.Lerp(midHealthColor, highHealthColor, (percentage - 0.5f) * 2f);
                else
                    fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, percentage * 2f);
            }
        }
    }
}