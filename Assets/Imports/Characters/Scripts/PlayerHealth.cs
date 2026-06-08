using UnityEngine;
using UnityEngine.UI;

namespace Abdulrahman.PlayerSystem
{
    public class PlayerHealth : MonoBehaviour
    {
        public event System.Action<float, float> OnHealthChanged;

        [Header("HEALTH")]
        public float maxHealth = 100f;
        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;

        [Header("DAMAGE VIGNETTE")]
        public Image damageVignette;
        public float vignetteMaxAlpha = 0.6f;
        public float vignetteFadeSpeed = 2f;
        private float _targetAlpha = 0f;

        private PlayerController _playerController;

        private void Start()
        {
            _currentHealth = maxHealth;
            _playerController = GetComponent<PlayerController>();

            if (damageVignette != null)
            {
                Color c = damageVignette.color;
                c.a = 0f;
                damageVignette.color = c;
            }

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Update()
        {
            HandleVignette();
        }

        private void HandleVignette()
        {
            if (damageVignette == null) return;

            float lowHealthAlpha = 1f - (_currentHealth / maxHealth);
            lowHealthAlpha = Mathf.Clamp(lowHealthAlpha * vignetteMaxAlpha, 0f, vignetteMaxAlpha);

            float finalAlpha = Mathf.Max(_targetAlpha, lowHealthAlpha);

            Color c = damageVignette.color;
            c.a = Mathf.Lerp(c.a, finalAlpha, Time.deltaTime * vignetteFadeSpeed);
            damageVignette.color = c;

            if (_targetAlpha > lowHealthAlpha)
                _targetAlpha = Mathf.Lerp(_targetAlpha, lowHealthAlpha, Time.deltaTime * vignetteFadeSpeed);
        }

        public void Heal(float amount)
        {
            _currentHealth += amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void HealPercentage(float percent)
        {
            float amount = maxHealth * (percent / 100f);
            Heal(amount);
        }

        public void TakeDamage(float amount, Vector3 knockbackDirection)
{
            if (_currentHealth <= 0) return;

            Debug.Log($"[PlayerHealth] Taking {amount} damage. Current Health: {_currentHealth - amount}");
            _currentHealth -= amount;
            _targetAlpha = vignetteMaxAlpha;

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_playerController != null)
            {
                Debug.Log($"[PlayerHealth] Applying knockback: {knockbackDirection}");
                _playerController.ApplyKnockback(knockbackDirection);
            }
            else
            {
                Debug.LogWarning("[PlayerHealth] _playerController is null, cannot apply knockback!");
            }

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }

        [Header("UI")]
        public GameObject deathScreen;

        private void Die()
        {
            Debug.Log("Player died");
            
            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
                
                // Unlock cursor so they can click buttons (if any)
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Stop time or other death logic
                Time.timeScale = 0f;
            }
        }
}
}