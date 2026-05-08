using OGT;
using System.Collections;
using UnityEngine;

public static class CanvasGroupExtensions
{
    public static Coroutine FadeIn(this CanvasGroup canvasGroup, float duration)
    {
        return CrossFadeAlpha(canvasGroup, canvasGroup.alpha, 1.0f, duration);
    }

    public static Coroutine FadeOut(this CanvasGroup canvasGroup, float duration)
    {
        return CrossFadeAlpha(canvasGroup, canvasGroup.alpha, 0.0f, duration);
    }

    private static Coroutine CrossFadeAlpha(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            canvasGroup.alpha = endAlpha;
            return null;
        }
#endif

        return CoroutineRunner.Instance.StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            float progress = 0.0f;

            while (progress < 1.0f)
            {
                progress += Time.deltaTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }
    }
}
