using UnityEngine;
using UnityEngine.UI;

namespace Abdulrahman.PlayerSystem
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("HEALTH")]
        public float maxHealth = 100f;
        private float _currentHealth;

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

        public void TakeDamage(float amount, Vector3 knockbackDirection)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;
            _targetAlpha = vignetteMaxAlpha;

            if (_playerController != null)
                _playerController.ApplyKnockback(knockbackDirection);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player died");
            // we'll add death logic later
        }
    }
}