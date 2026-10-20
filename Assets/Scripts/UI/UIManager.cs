
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class UIPage
    {
        public string pageName;
        public GameObject panel;
        [Tooltip("页面自带返回按钮时使用；留空则使用公共返回按钮。")]
        public Button localBackButton;
    }

    [Header("UI页面")]
    public List<UIPage> pages = new List<UIPage>();

    [Header("初始页面")]
    public string initialPage = "Login";

    [Header("返回按钮")]
    public Button backButton;

    [Header("黑屏转场")]
    public CanvasGroup fadeOverlay;

    public float fadeOutDuration = 0.2f;
    public float fadeInDuration = 0.3f;

    private readonly Stack<string> history = new Stack<string>();

    private string currentPage;
    private bool isTransitioning = false;

    void Start()
    {
        // 初始化黑屏遮罩
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }

        // 初始化所有页面为隐藏
        foreach (UIPage page in pages)
        {
            if (page.panel != null)
                page.panel.SetActive(false);
        }

        // 默认从登录页面开始
        string startPage = initialPage;

        // 如果是从3D箱庭返回
        if (SceneController.ReturnPage == "YanMap")
        {
            startPage = "YanMap";

            // 消费返回目标，避免下次仍然进入 YanMap
            SceneController.ReturnPage = "Login";

            // 恢复页面历史
            history.Push("Login");
            history.Push("WorldMap");
        }

        // 显示目标页面
        ShowPage(startPage);

        // 返回按钮在场景的 On Click 中绑定，避免与持久化事件重复注册。
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            GoBack();
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
            GoBack();
#endif
    }

    // 打开新页面
    public void OpenPage(string pageName)
    {
        if (isTransitioning) return;
        if (pageName == currentPage) return;
        if (FindPage(pageName) == null) return;

        StartCoroutine(TransitionTo(pageName, false));
    }

    // 返回上一级
    public void GoBack()
    {
        if (isTransitioning || history.Count == 0)
            return;

        StartCoroutine(TransitionTo(history.Peek(), true));
    }

    // 黑屏切换
    IEnumerator TransitionTo(string targetPage, bool isBack)
    {
        isTransitioning = true;

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true;
            yield return Fade(0f, 1f, fadeOutDuration);
        }

        // 更新历史
        if (isBack)
        {
            history.Pop();
        }
        else if (!string.IsNullOrEmpty(currentPage))
        {
            history.Push(currentPage);
        }

        // 切换页面
        ShowPage(targetPage);

        if (fadeOverlay != null)
        {
            yield return Fade(1f, 0f, fadeInDuration);
            fadeOverlay.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    // 控制透明度
    IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            fadeOverlay.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            fadeOverlay.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeOverlay.alpha = to;
    }

    // 查找页面
    GameObject FindPage(string pageName)
    {
        foreach (UIPage page in pages)
        {
            if (page.pageName == pageName)
                return page.panel;
        }

        return null;
    }

    // 显示指定页面
    void ShowPage(string pageName)
    {
        GameObject target = FindPage(pageName);

        if (target == null)
        {
            Debug.LogError("找不到UI页面：" + pageName);
            return;
        }

        foreach (UIPage page in pages)
        {
            if (page.panel != null)
                page.panel.SetActive(page.panel == target);
        }

        currentPage = pageName;

        // 页面自带返回热点时，隐藏公共按钮，避免两套导航画面重叠。
        UIPage activePage = pages.Find(page => page.panel == target);
        Button localBack = activePage != null ? activePage.localBackButton : null;
        if (backButton != null)
            backButton.gameObject.SetActive(history.Count > 0 && localBack == null);
        if (localBack != null)
            localBack.gameObject.SetActive(history.Count > 0);
    }
}
