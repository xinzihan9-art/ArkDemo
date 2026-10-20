
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("首次交互剧情图片")]
    public GameObject storyImagePanel;

    [Header("后续交互选项菜单")]
    public GameObject interactionMenuPanel;

    [Header("商店采购界面")]
    public GameObject shopPanel;

    [Header("商店管理器")]
    public ShopManager shopManager;

    private bool isDialogueOpen = false;

    void Start()
    {
        // 游戏开始时关闭所有界面
        CloseAllPanels();
    }

    // 第一次交互：打开剧情图片
    public void OpenDialogue()
    {
        if (storyImagePanel == null)
            return;

        CloseAllPanels();

        storyImagePanel.SetActive(true);
        isDialogueOpen = true;

        Debug.Log("首次交互：打开剧情图片");
    }

    // 第二次及以后：打开交互选项菜单
    public void OpenInteractionMenu()
    {
        if (interactionMenuPanel == null)
            return;

        CloseAllPanels();

        interactionMenuPanel.SetActive(true);
        isDialogueOpen = true;

        Debug.Log("打开NPC交互选项");
    }

    // 点击采购商品：打开商店
    public void OpenShopPanel()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("ShopPanel未绑定！");
            return;
        }

        CloseAllPanels();

        // 每次打开商店时清除商品选择
        if (shopManager != null)
            shopManager.ClearSelection();

        shopPanel.SetActive(true);
        isDialogueOpen = true;

        Debug.Log("商店采购界面已打开");
    }

    // 关闭当前交互界面
    public void CloseDialogue()
    {
        CloseAllPanels();

        Debug.Log("交互界面已关闭");
    }

    // 查询是否存在打开的交互窗口
    public bool IsDialogueOpen()
    {
        return isDialogueOpen;
    }

    // 统一关闭所有交互界面
    private void CloseAllPanels()
    {
        if (storyImagePanel != null)
            storyImagePanel.SetActive(false);

        if (interactionMenuPanel != null)
            interactionMenuPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        isDialogueOpen = false;
    }
}


