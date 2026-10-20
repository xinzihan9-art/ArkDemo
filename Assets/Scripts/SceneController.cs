
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 返回主UI后恢复的页面
    public static string ReturnPage = "Login";

    // 是否需要在新场景中执行黑屏淡出
    private static bool fadeInOnLoad = false;

    [Header("场景转场")]
    public CanvasGroup sceneFadeOverlay;
    public CanvasGroup blackOverlay;

    [Header("龙门主题图片（仅MainUIScene需要）")]
    public CanvasGroup transitionImage;

    [Header("时间参数")]
    public float imageFadeDuration = 0.3f;
    public float imageHoldDuration = 0.8f;
    public float fadeOutDuration = 0.3f;
    public float fadeInDuration = 0.3f;

    private bool isTransitioning = false;

    void Awake()
    {
        if (sceneFadeOverlay == null) return;

        // 新场景加载后保持全黑，避免闪屏
        sceneFadeOverlay.alpha = fadeInOnLoad ? 1f : 0f;
        sceneFadeOverlay.blocksRaycasts = fadeInOnLoad;

        if (blackOverlay != null)
            blackOverlay.alpha = fadeInOnLoad ? 1f : 0f;

        if (transitionImage != null)
            transitionImage.alpha = 0f;
    }

    IEnumerator Start()
    {
        if (fadeInOnLoad && sceneFadeOverlay != null)
        {
            fadeInOnLoad = false;

            yield return Fade(
                blackOverlay,
                1f,
                0f,
                fadeInDuration
            );

            sceneFadeOverlay.alpha = 0f;
            sceneFadeOverlay.blocksRaycasts = false;
        }
    }

    // 进入龙门：图片 + 黑屏
    public void EnterLongmen()
    {
        if (!isTransitioning)
            StartCoroutine(EnterLongmenTransition());
    }

    IEnumerator EnterLongmenTransition()
    {
        isTransitioning = true;

        ReturnPage = "YanMap";

        sceneFadeOverlay.alpha = 1f;
        sceneFadeOverlay.blocksRaycasts = true;

        // 1. 龙门图片淡入
        if (transitionImage != null)
        {
            yield return Fade(
                transitionImage,
                0f,
                1f,
                imageFadeDuration
            );
        }

        // 2. 停留一会
        yield return new WaitForSecondsRealtime(
            imageHoldDuration
        );

        // 3. 渐变黑屏
        yield return Fade(
            blackOverlay,
            0f,
            1f,
            fadeOutDuration
        );

        // 4. 加载箱庭
        fadeInOnLoad = true;
        SceneManager.LoadScene("GameModeScene");
    }

    // 返回YanMap：仅黑屏
    public void ReturnToMap()
    {
        if (!isTransitioning)
            StartCoroutine(ReturnTransition());
    }

    IEnumerator ReturnTransition()
    {
        isTransitioning = true;

        sceneFadeOverlay.alpha = 1f;
        sceneFadeOverlay.blocksRaycasts = true;

        // 直接黑屏
        yield return Fade(
            blackOverlay,
            0f,
            1f,
            fadeOutDuration
        );

        fadeInOnLoad = true;
        SceneManager.LoadScene("MainUIScene");
    }

    // 通用Alpha动画
    IEnumerator Fade(
        CanvasGroup group,
        float from,
        float to,
        float duration
    )
    {
        if (group == null) yield break;

        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            group.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        group.alpha = to;
    }
}
