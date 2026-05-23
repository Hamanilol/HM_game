using UnityEngine;

namespace Abdulrahman.EnemySystem
{
    public class ZombieHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float _currentHealth;
        private ZombieAI _zombieAI;

        private void Start()
        {
            _currentHealth = maxHealth;
            _zombieAI = GetComponent<ZombieAI>();
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _zombieAI.Die();
            }
        }
    }
}