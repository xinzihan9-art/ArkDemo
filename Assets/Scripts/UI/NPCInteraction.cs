
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [Header("交互UI")]
    public GameObject interactionPrompt;

    [Header("对话管理器")]
    public DialogueManager dialogueManager;

    [Header("首次交互状态")]
    public GameObject exclamationMark;

    private bool playerNearby = false;
    private bool hasInteracted = false;

    void Start()
    {
        playerNearby = false;
        hasInteracted = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (exclamationMark != null)
            exclamationMark.SetActive(true);
    }

    void Update()
    {
        if (!playerNearby || dialogueManager == null)
            return;

        if (dialogueManager.IsDialogueOpen())
            return;

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (!hasInteracted)
            {
                // 第一次交互：展示剧情图片
                dialogueManager.OpenDialogue();
                Debug.Log("首次交互：打开剧情图片");
            }
            else
            {
                // 第二次及以后：展示选项菜单
                dialogueManager.OpenInteractionMenu();
                Debug.Log("后续交互：打开选项菜单");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;
        RefreshInteractionPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;
        RefreshInteractionPrompt();
    }

    // 首次剧情图片关闭后调用
    public void RefreshPrompt()
    {
        if (!hasInteracted)
        {
            hasInteracted = true;

            if (exclamationMark != null)
                exclamationMark.SetActive(false);

            Debug.Log("贸易点已解锁！");
        }

        RefreshInteractionPrompt();
    }

    // 普通菜单关闭后调用
    public void RefreshInteractionPrompt()
    {
        if (interactionPrompt == null)
            return;

        bool shouldShow =
            playerNearby &&
            dialogueManager != null &&
            !dialogueManager.IsDialogueOpen();

        interactionPrompt.SetActive(shouldShow);
    }
}

