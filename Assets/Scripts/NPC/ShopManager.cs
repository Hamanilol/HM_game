using UnityEngine;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.NPC
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        public GameObject shopUI;
        private PlayerHealth _playerHealth;
        private PlayerController _playerController;
        private CurrencyUI _currencyUI;

        private void Awake()
        {
            Instance = this;
            if (shopUI != null) shopUI.SetActive(false);
        }

        private void Start()
        {
            _currencyUI = FindFirstObjectByType<CurrencyUI>();
        }

        private void FindPlayer()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                _playerHealth = p.GetComponent<PlayerHealth>();
                _playerController = p.GetComponent<PlayerController>();
            }
        }

        public void OpenShop()
        {
            if (shopUI != null)
            {
                shopUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                FindPlayer();
            }
        }

        public void CloseShop()
        {
            if (shopUI != null)
            {
                shopUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void BuyHeal10Percent()
        {
            if (TrySpend(10))
            {
                _playerHealth.HealPercentage(10f);
                Debug.Log("Bought 10% Heal");
            }
        }

        public void BuyHeal50HP()
        {
            if (TrySpend(30))
            {
                _playerHealth.Heal(50f);
                Debug.Log("Bought 50 HP Heal");
            }
        }

        public void BuyHeal100HP()
        {
            if (TrySpend(60))
            {
                _playerHealth.Heal(100f);
                Debug.Log("Bought 100 HP Heal");
            }
        }

        public void BuyJumpBoost()
        {
            if (TrySpend(150))
            {
                _playerController.BoostJump(1.5f, 30f);
                Debug.Log("Bought Jump Boost");
            }
        }

        public void BuyDamageBoost()
        {
            if (TrySpend(130))
            {
                StartCoroutine(DamageBoostCoroutine(1.2f, 30f));
                Debug.Log("Bought Damage Boost");
            }
        }

        private System.Collections.IEnumerator DamageBoostCoroutine(float multiplier, float duration)
        {
            BaseWeapon.GlobalDamageMultiplier *= multiplier;
            yield return new WaitForSeconds(duration);
            BaseWeapon.GlobalDamageMultiplier /= multiplier;
        }

        private bool TrySpend(int amount)
        {
            // CurrencyUI uses PlayerPrefs, but we should access the actual script to check and spend.
            // Since AddMoney and RemoveMoney are public, we'll use them.
            // However, we need to check if player has enough money.
            
            int currentMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
            if (currentMoney >= amount)
            {
                if (_currencyUI != null) _currencyUI.RemoveMoney(amount);
                else
                {
                    // Fallback if script not found (shouldn't happen with additive loading)
                    PlayerPrefs.SetInt("PlayerMoney", currentMoney - amount);
                    PlayerPrefs.Save();
                }
                return true;
            }
            else
            {
                Debug.Log("Not enough money!");
                return false;
            }
        }
    }
}
