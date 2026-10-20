
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeManager : MonoBehaviour
{
    [System.Serializable]
    public class TradeProduct
    {
        public string productName;
        public int stock = 20;
        public int sellPrice = 3300;
        public int buyPrice = 3000;
    }

    [Header("商品数据")]
    public TradeProduct product01 = new TradeProduct();
    public TradeProduct product02 = new TradeProduct();

    [Header("仓库商品按钮")]
    public Button productButton01;
    public Button productButton02;

    [Header("商品选中边框")]
    public GameObject selectionBorder01;
    public GameObject selectionBorder02;

    [Header("价格文本")]
    public TMP_Text sellPriceText;
    public TMP_Text buyPriceText;

    [Header("数量操作")]
    public Button minusButton;
    public Button plusButton;
    public Button maxButton;
    public TMP_Text quantityText;

    [Header("预计收益")]
    public TMP_Text totalPriceText;

    [Header("底部按钮")]
    public Button confirmSellButton;
    public Button cancelButton;

    [Header("UI管理器")]
    public UIManager uiManager;

    [Header("玩家龙门币（Demo数据）")]
    public int playerMoney = 0;

    private int selectedProduct = -1;
    private int sellQuantity = 0;

    void Start()
    {
        // 初始化商品按钮
        BindButton(productButton01, SelectProduct01);
        BindButton(productButton02, SelectProduct02);

        // 初始化数量按钮
        BindButton(minusButton, DecreaseQuantity);
        BindButton(plusButton, IncreaseQuantity);
        BindButton(maxButton, SetMaxQuantity);

        // 初始化底部按钮
        BindButton(confirmSellButton, ConfirmSell);
        BindButton(cancelButton, CancelTrade);

        // 默认不选中商品
        selectedProduct = -1;
        sellQuantity = 0;

        UpdateUI();
    }

    // 统一绑定按钮
    void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    void SelectProduct01()
    {
        SelectProduct(0);
    }

    void SelectProduct02()
    {
        SelectProduct(1);
    }

    // 选择商品
    void SelectProduct(int index)
    {
        selectedProduct = index;
        sellQuantity = 0;

        Debug.Log("当前选中商品：" + (index + 1));

        UpdateUI();
    }

    // 获取当前商品
    TradeProduct GetSelectedProduct()
    {
        switch (selectedProduct)
        {
            case 0:
                return product01;
            case 1:
                return product02;
            default:
                return null;
        }
    }

    // 减少数量
    void DecreaseQuantity()
    {
        if (GetSelectedProduct() == null)
            return;

        sellQuantity = Mathf.Max(0, sellQuantity - 1);

        UpdateUI();
    }

    // 增加数量
    void IncreaseQuantity()
    {
        TradeProduct product = GetSelectedProduct();

        if (product == null)
            return;

        sellQuantity = Mathf.Min(
            Mathf.Max(0, product.stock),
            sellQuantity + 1
        );

        UpdateUI();
    }

    // 最大数量
    void SetMaxQuantity()
    {
        TradeProduct product = GetSelectedProduct();

        if (product == null)
            return;

        sellQuantity = Mathf.Max(0, product.stock);

        UpdateUI();
    }

    // 确认售卖
    void ConfirmSell()
    {
        TradeProduct product = GetSelectedProduct();

        if (product == null || sellQuantity <= 0)
        {
            Debug.Log("请先选择商品和出售数量");
            return;
        }

        if (sellQuantity > product.stock)
        {
            Debug.LogWarning("商品库存不足");
            UpdateUI();
            return;
        }

        int total = sellQuantity * Mathf.Max(0, product.sellPrice);

        product.stock -= sellQuantity;
        playerMoney += total;

        Debug.Log(
            "出售成功：" + product.productName +
            "，数量：" + sellQuantity +
            "，获得龙门币：" + total +
            "，当前龙门币：" + playerMoney
        );

        sellQuantity = 0;

        UpdateUI();
    }

    // 取消交易
    void CancelTrade()
    {
        if (uiManager != null)
            uiManager.GoBack();
        else
            Debug.LogWarning("未绑定 UIManager");
    }

    // 更新整个贸易界面
    void UpdateUI()
    {
        TradeProduct product = GetSelectedProduct();

        // 更新选中边框
        if (selectionBorder01 != null)
            selectionBorder01.SetActive(selectedProduct == 0);

        if (selectionBorder02 != null)
            selectionBorder02.SetActive(selectedProduct == 1);

        // 更新左右价格
        if (sellPriceText != null)
            sellPriceText.text = Mathf.Max(0, product01.sellPrice).ToString();

        if (buyPriceText != null)
            buyPriceText.text = Mathf.Max(0, product02.buyPrice).ToString();

        // 更新数量
        if (quantityText != null)
            quantityText.text = sellQuantity.ToString();

        // 更新预计收益
        int total = product == null
            ? 0
            : sellQuantity * Mathf.Max(0, product.sellPrice);

        if (totalPriceText != null)
            totalPriceText.text = total.ToString();

        // 更新数量按钮状态
        bool hasProduct = product != null;
        int stock = hasProduct ? Mathf.Max(0, product.stock) : 0;

        if (minusButton != null)
            minusButton.interactable = hasProduct && sellQuantity > 0;

        if (plusButton != null)
            plusButton.interactable = hasProduct && sellQuantity < stock;

        if (maxButton != null)
            maxButton.interactable = hasProduct && stock > 0;

        if (confirmSellButton != null)
            confirmSellButton.interactable =
                hasProduct && sellQuantity > 0 &&
                sellQuantity <= stock;
    }

    // 后续随机价格系统可以调用
    public void SetPrices(
        int product01SellPrice,
        int product02BuyPrice
    )
    {
        product01.sellPrice = Mathf.Max(0, product01SellPrice);
        product02.buyPrice = Mathf.Max(0, product02BuyPrice);

        UpdateUI();
    }
}

