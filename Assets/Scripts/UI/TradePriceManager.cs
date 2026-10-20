
using UnityEngine;

public class TradePriceManager : MonoBehaviour
{
    [Header("基础价格")]
    public int product01BasePrice = 3000;
    public int product02BasePrice = 3000;

    [Header("TradeManager")]
    public TradeManager tradeManager;

    [Header("测试：启动时生成价格")]
    public bool generateOnStart = true;

    void Start()
    {
        if (generateOnStart)
            GeneratePrices();
    }

    // 玩家购买价格：80%低价，20%高价
    public int GenerateBuyPrice(int basePrice)
    {
        float roll = Random.value;
        float multiplier;

        if (roll < 0.8f)
        {
            multiplier = Random.Range(0.6f, 1.0f);
        }
        else
        {
            multiplier = Random.Range(1.0f, 1.2f);
        }

        return Mathf.RoundToInt(basePrice * multiplier);
    }

    // 玩家出售价格：10%普通，70%中位，20%高价
    public int GenerateSellPrice(int basePrice)
    {
        float roll = Random.value;
        float multiplier;

        if (roll < 0.1f)
        {
            multiplier = Random.Range(0.8f, 1.2f);
        }
        else if (roll < 0.8f)
        {
            multiplier = Random.Range(1.2f, 2.0f);
        }
        else
        {
            multiplier = Random.Range(2.0f, 2.4f);
        }

        return Mathf.RoundToInt(basePrice * multiplier);
    }

    // 为贸易页面生成并设置价格
    public void GeneratePrices()
    {
        int sellPrice = GenerateSellPrice(product01BasePrice);
        int buyPrice = GenerateBuyPrice(product02BasePrice);

        if (tradeManager != null)
        {
            tradeManager.SetPrices(sellPrice, buyPrice);
        }
        else
        {
            Debug.LogWarning("TradePriceManager 未绑定 TradeManager");
        }

        Debug.Log(
            $"贸易价格生成：收购价 {sellPrice}，购买价 {buyPrice}"
        );
    }
}

