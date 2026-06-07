using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI eventText;

    [Header("Money Settings")]
    public int startingMoney = 0;

    [Header("Bonus Event")]
    public int ghostBonusMin = 100;
    public int ghostBonusMax = 300;
    public float ghostEventChance = 0.2f;

    private int money;
    private int comboMultiplier = 1;

    // =========================
    // START
    // =========================

    void Start()
    {
        // RESET MONEY ON SCENE LOAD
        money = startingMoney;
        SaveMoney(); // Reset the saved value as well if needed

        UpdateUI();

        if (eventText != null)
        {
            eventText.gameObject.SetActive(false);
        }
    }

    // =========================
    // ADD MONEY
    // =========================

    public void AddMoney(int amount)
    {
        int finalAmount = amount * comboMultiplier;

        money += finalAmount;

        // RANDOM GHOST BONUS EVENT
        float randomChance = Random.Range(0f, 1f);

        if (randomChance <= ghostEventChance)
        {
            int bonus = Random.Range(ghostBonusMin, ghostBonusMax + 1);

            money += bonus;

            ShowEvent("GHOST TREASURE FOUND! +$" + bonus);
        }

        SaveMoney();
        UpdateUI();
    }

    // =========================
    // REMOVE MONEY
    // =========================

    public void RemoveMoney(int amount)
    {
        money -= amount;

        if (money < 0)
        {
            money = 0;
        }

        SaveMoney();
        UpdateUI();
    }

    // =========================
    // MULTIPLIER
    // =========================

    public void SetMultiplier(int multiplier)
    {
        comboMultiplier = multiplier;

        ShowEvent("2X MONEY BONUS!");
    }

    public void ResetMultiplier()
    {
        comboMultiplier = 1;
    }

    // =========================
    // SAVE MONEY
    // =========================

    void SaveMoney()
    {
        PlayerPrefs.SetInt("PlayerMoney", money);
        PlayerPrefs.Save();
    }

    // =========================
    // UPDATE UI
    // =========================

    void UpdateUI()
    {
        moneyText.text = money.ToString();
    }

    // =========================
    // EVENT POPUP
    // =========================

    void ShowEvent(string message)
    {
        if (eventText == null)
            return;

        eventText.gameObject.SetActive(true);

        eventText.text = message;

        CancelInvoke(nameof(HideEvent));

        Invoke(nameof(HideEvent), 2f);
    }

    void HideEvent()
    {
        eventText.gameObject.SetActive(false);
    }
}