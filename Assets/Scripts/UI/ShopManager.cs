
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopProduct
    {
        public string productName;
        public int price;
        public int stock;
    }

    [Header("商品配置")]
    public ShopProduct[] products =
    {
        new ShopProduct
        {
            productName = "兔兔鲜蔬礼盒",
            price = 3300,
            stock = 20
        },
        new ShopProduct
        {
            productName = "应急理智合剂",
            price = 6000,
            stock = 1
        }
    };

    [Header("单次购买上限")]
    [Min(1)]
    public int maxPurchaseLimit = 10;

    [Header("商品按钮")]
    public Button productButton01;
    public Button productButton02;

    [Header("选中高亮边框")]
    public GameObject selectionBorder01;
    public GameObject selectionBorder02;

    [Header("数量控制按钮")]
    public Button plusButton;
    public Button minusButton;
    public Button maxButton;

    [Header("购买按钮")]
    public Button buyButton;

    [Header("动态文本")]
    public TMP_Text quantityText;
    public TMP_Text totalPriceText;

    [Header("玩家数据")]
    public PlayerDataManager playerData;

    // -1 表示未选中商品
    private int selectedProductId = -1;

    // 当前购买数量
    private int selectedQuantity = 0;

    void Awake()
    {
        // 自动绑定商品按钮
        if (productButton01 != null)
            productButton01.onClick.AddListener(SelectProduct01);

        if (productButton02 != null)
            productButton02.onClick.AddListener(SelectProduct02);

        // 自动绑定数量按钮
        if (plusButton != null)
            plusButton.onClick.AddListener(IncreaseQuantity);

        if (minusButton != null)
            minusButton.onClick.AddListener(DecreaseQuantity);

        if (maxButton != null)
            maxButton.onClick.AddListener(SelectMaxQuantity);

        // 自动绑定购买按钮
        if (buyButton != null)
            buyButton.onClick.AddListener(Purchase);

        ClearSelection();
    }

    void Start()
    {
        RefreshUI();
    }

    // 选中第一个商品
    public void SelectProduct01()
    {
        SelectProduct(0);
    }

    // 选中第二个商品
    public void SelectProduct02()
    {
        SelectProduct(1);
    }

    // 选择商品
    public void SelectProduct(int id)
    {
        if (products == null ||
            id < 0 ||
            id >= products.Length)
        {
            return;
        }

        selectedProductId = id;

        // 切换商品后数量归零
        selectedQuantity = 0;

        RefreshUI();

        Debug.Log("当前选中商品：" + products[id].productName);
    }

    // 清除选中状态
    public void ClearSelection()
    {
        selectedProductId = -1;
        selectedQuantity = 0;

        RefreshUI();
    }

    // 获取当前选中商品
    public int GetSelectedProductId()
    {
        return selectedProductId;
    }

    // 计算最大可购买数量
    private int GetMaxQuantity()
    {
        if (products == null ||
            selectedProductId < 0 ||
            selectedProductId >= products.Length)
        {
            return 0;
        }

        ShopProduct product = products[selectedProductId];

        if (product.price <= 0 || playerData == null)
            return 0;

        // 玩家资金允许购买的数量
        int affordable = playerData.lmd / product.price;

        // 同时限制单次购买上限、库存、资金
        return Mathf.Max(
            0,
            Mathf.Min(
                maxPurchaseLimit,
                Mathf.Min(product.stock, affordable)
            )
        );
    }

    // 点击 +
    public void IncreaseQuantity()
    {
        if (selectedProductId < 0)
            return;

        int max = GetMaxQuantity();

        selectedQuantity = Mathf.Min(
            selectedQuantity + 1,
            max
        );

        RefreshUI();

        Debug.Log("当前购买数量：" + selectedQuantity);
    }

    // 点击 -
    public void DecreaseQuantity()
    {
        selectedQuantity = Mathf.Max(
            0,
            selectedQuantity - 1
        );

        RefreshUI();

        Debug.Log("当前购买数量：" + selectedQuantity);
    }

    // 点击最大
    public void SelectMaxQuantity()
    {
        selectedQuantity = GetMaxQuantity();

        RefreshUI();

        Debug.Log("最大购买数量：" + selectedQuantity);
    }

    // 点击购买
    public void Purchase()
    {
        if (products == null ||
            selectedProductId < 0 ||
            selectedProductId >= products.Length ||
            playerData == null)
        {
            Debug.LogWarning("购买失败：商品或玩家数据未配置");
            return;
        }

        if (selectedQuantity <= 0 ||
            selectedQuantity > GetMaxQuantity())
        {
            Debug.LogWarning("购买失败：数量不合法");
            return;
        }

        ShopProduct product = products[selectedProductId];

        int cost = product.price * selectedQuantity;

        // 玩家扣款、仓库增加商品
        bool success = playerData.TryPurchase(
            selectedProductId,
            selectedQuantity,
            cost
        );

        if (!success)
        {
            Debug.LogWarning("购买失败：玩家资金不足");
            return;
        }

        // 扣除商店库存
        product.stock -= selectedQuantity;

        Debug.Log(
            $"购买成功：{product.productName} × {selectedQuantity}，" +
            $"花费：{cost}，剩余库存：{product.stock}"
        );

        // 购买完成后数量清零
        selectedQuantity = 0;

        RefreshUI();
    }

    // 刷新商店界面
    private void RefreshUI()
    {
        // 更新商品高亮
        if (selectionBorder01 != null)
            selectionBorder01.SetActive(selectedProductId == 0);

        if (selectionBorder02 != null)
            selectionBorder02.SetActive(selectedProductId == 1);

        // 更新购买数量
        if (quantityText != null)
            quantityText.text = selectedQuantity.ToString();

        // 计算总价
        int totalPrice = 0;

        if (products != null &&
            selectedProductId >= 0 &&
            selectedProductId < products.Length)
        {
            totalPrice =
                products[selectedProductId].price *
                selectedQuantity;
        }

        // 更新总价文字
        if (totalPriceText != null)
            totalPriceText.text = totalPrice.ToString("N0");

        // 更新按钮状态
        int maxQuantity = GetMaxQuantity();

        if (plusButton != null)
            plusButton.interactable =
                selectedProductId >= 0 &&
                selectedQuantity < maxQuantity;

        if (minusButton != null)
            minusButton.interactable =
                selectedQuantity > 0;

        if (maxButton != null)
            maxButton.interactable =
                selectedProductId >= 0 &&
                maxQuantity > 0;

        if (buyButton != null)
            buyButton.interactable =
                selectedProductId >= 0 &&
                selectedQuantity > 0 &&
                selectedQuantity <= maxQuantity;
    }

    // 供其他系统读取商品剩余库存
    public int GetProductStock(int productId)
    {
        if (products == null ||
            productId < 0 ||
            productId >= products.Length)
        {
            return 0;
        }

        return products[productId].stock;
    }
}




