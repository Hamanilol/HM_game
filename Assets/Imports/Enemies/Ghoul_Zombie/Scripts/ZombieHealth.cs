using UnityEngine;

namespace Abdulrahman.EnemySystem
{
    public class EnemyHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float _currentHealth;
        private BaseEnemyAI _enemyAI;

        public event System.Action<float, float> OnHealthChanged;

        private void Start()
        {
            _currentHealth = maxHealth;
            _enemyAI = GetComponent<BaseEnemyAI>();
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _enemyAI.Die();
            }
        }
}
}