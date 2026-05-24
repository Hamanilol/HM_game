using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    private int money = 0;

    void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        moneyText.text = "$" + money;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddMoney(50);
        }
    }
}