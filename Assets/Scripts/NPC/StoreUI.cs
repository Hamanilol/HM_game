using UnityEngine;
using TMPro;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.NPC
{
    /// <summary>
    /// Healing store. Sells a Bandage (+5 HP) and a First Aid Kit (+50 HP).
    /// Spends the player's actual currency (PlayerPrefs "PlayerMoney", managed by CurrencyUI).
    /// Shows "U need more points" when the budget is lower than the cost.
    /// Player HP never exceeds PlayerHealth.maxHealth (Heal() clamps it).
    /// </summary>
    public class StoreUI : MonoBehaviour
    {
        [Header("Store Panel")]
        public GameObject storePanel;
        public KeyCode toggleKey = KeyCode.B;
        public bool handleCursor = true;

        [Header("Item Prices (vary per item)")]
        public int bandagePrice = 25;
        public int firstAidPrice = 100;

        [Header("Item Heal Amounts")]
        public float bandageHeal = 5f;
        public float firstAidHeal = 50f;

        [Header("UI References")]
        public TextMeshProUGUI budgetText;
        public TextMeshProUGUI messageText;
        public TextMeshProUGUI bandagePriceText;
        public TextMeshProUGUI firstAidPriceText;

        private const string MoneyKey = "PlayerMoney";

        private PlayerHealth _playerHealth;
        private CurrencyUI _currencyUI;

        private void Start()
        {
            RefreshReferences();

            if (bandagePriceText != null) bandagePriceText.text = bandagePrice + " pts";
            if (firstAidPriceText != null) firstAidPriceText.text = firstAidPrice + " pts";
            if (messageText != null) messageText.gameObject.SetActive(false);

            if (storePanel != null) storePanel.SetActive(false);
        }

        private void Update()
        {
            if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
            {
                if (storePanel == null) return;
                if (storePanel.activeSelf) Close();
                else Open();
            }
        }

        private void RefreshReferences()
        {
            if (_currencyUI == null) _currencyUI = FindFirstObjectByType<CurrencyUI>();
            if (_playerHealth == null) _playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        public void Open()
        {
            RefreshReferences();
            if (storePanel != null) storePanel.SetActive(true);
            if (messageText != null) messageText.gameObject.SetActive(false);

            if (handleCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            UpdateBudgetUI();
        }

        public void Close()
        {
            if (storePanel != null) storePanel.SetActive(false);

            if (handleCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Hooked to the Bandage Buy button.
        public void BuyBandage()
        {
            TryBuy(bandagePrice, bandageHeal, "Bandage");
        }

        // Hooked to the First Aid Kit Buy button.
        public void BuyFirstAidKit()
        {
            TryBuy(firstAidPrice, firstAidHeal, "First Aid Kit");
        }

        private void TryBuy(int price, float healAmount, string itemName)
        {
            RefreshReferences();

            int budget = GetBudget();
            if (budget < price)
            {
                ShowMessage("U need more points");
                return;
            }

            SpendBudget(price);

            if (_playerHealth != null)
            {
                // Heal() internally clamps so HP never exceeds maxHealth (100).
                _playerHealth.Heal(healAmount);
            }

            ShowMessage(itemName + " purchased!  +" + healAmount + " HP");
            UpdateBudgetUI();
        }

        private int GetBudget()
        {
            return PlayerPrefs.GetInt(MoneyKey, 0);
        }

        private void SpendBudget(int amount)
        {
            if (_currencyUI != null)
            {
                _currencyUI.RemoveMoney(amount);
            }
            else
            {
                int current = PlayerPrefs.GetInt(MoneyKey, 0);
                current = Mathf.Max(0, current - amount);
                PlayerPrefs.SetInt(MoneyKey, current);
                PlayerPrefs.Save();
            }
        }

        private void UpdateBudgetUI()
        {
            if (budgetText != null) budgetText.text = "Points: " + GetBudget();
        }

        private void ShowMessage(string msg)
        {
            if (messageText == null) return;
            messageText.gameObject.SetActive(true);
            messageText.text = msg;
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), 2.5f);
        }

        private void HideMessage()
        {
            if (messageText != null) messageText.gameObject.SetActive(false);
        }
    }
}
