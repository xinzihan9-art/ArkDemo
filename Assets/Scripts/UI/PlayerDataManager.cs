
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    [Header("测试初始资金")]
    public int lmd = 50000;

    [Header("玩家仓库")]
    public int vegetableGiftBox = 0;
    public int sanityPotion = 0;

    // 购买交易：验证金额并更新仓库
    public bool TryPurchase(int productId, int quantity, int cost)
    {
        if (quantity <= 0 || cost < 0 || lmd < cost)
            return false;

        if (productId != 0 && productId != 1)
            return false;

        lmd -= cost;

        if (productId == 0)
            vegetableGiftBox += quantity;
        else
            sanityPotion += quantity;

        Debug.Log(
            $"购买成功！商品ID:{productId} " +
            $"数量:{quantity} 花费:{cost} 剩余龙门币:{lmd}"
        );

        return true;
    }
}
