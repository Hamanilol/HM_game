using UnityEngine;
using Abdulrahman.EnemySystem;

namespace Abdulrahman.EnemySystem
{
    public class EnemyHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        [Header("REWARD")]
        public int moneyReward = 100;
        
        [Header("BOSS SETTINGS")]
        public bool isBoss = false;
        public string bossDisplayName = "Boss";

        public float _currentHealth;
        private BaseEnemyAI _enemyAI;
        private CurrencyUI _currencyUI;

        public event System.Action<float, float> OnHealthChanged;

        private void Start()
        {
            _currentHealth = maxHealth;
            _enemyAI = GetComponent<BaseEnemyAI>();
            _currencyUI = FindFirstObjectByType<CurrencyUI>();
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (isBoss)
            {
                var bossBar = Object.FindAnyObjectByType<Abdulrahman.UISystem.BossHealthBar>(FindObjectsInactive.Include);
                if (bossBar != null)
                {
                    bossBar.InitializeBoss(this, bossDisplayName);
                }
            }
        }

        public static void DealDamageToEnemies(GameObject target, float amount)
        {
            if (target == null) return;
            
            // Look for health component on the object or its parents
            EnemyHealth health = target.GetComponentInParent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(amount);
            }
        }

        public void TakeDamage(float amount)
{
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;
            Debug.Log($"[EnemyHealth] {gameObject.name} took {amount} damage. HP: {_currentHealth}/{maxHealth}");
            
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                
                if (_currencyUI != null)
                    _currencyUI.AddMoney(moneyReward);
                
                if (_enemyAI != null)
                    _enemyAI.Die();
                else
                    Destroy(gameObject);
            }
        }
    }
}