using UnityEngine;

namespace Abdulrahman.EnemySystem
{
    public class EnemyHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float _currentHealth;
        private BaseEnemyAI _enemyAI;

        private void Start()
        {
            _currentHealth = maxHealth;
            _enemyAI = GetComponent<BaseEnemyAI>();
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _enemyAI.Die();
            }
        }
    }
}