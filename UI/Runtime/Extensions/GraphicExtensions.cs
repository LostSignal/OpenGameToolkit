using OGT;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicExtensions
{
    public static Coroutine FadeIn(this Graphic graphic, float duration)
    {
        return CrossFadeAlpha(graphic, graphic.color.a, 1.0f, duration);
    }

    public static Coroutine FadeOut(this Graphic graphic, float duration)
    {
        return CrossFadeAlpha(graphic, graphic.color.a, 0.0f, duration);
    }

    private static Coroutine CrossFadeAlpha(Graphic graphic, float startAlpha, float endAlpha, float duration)
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            graphic.color = graphic.color.SetA(endAlpha);
            return null;
        }
#endif

        return CoroutineRunner.Instance.StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            Color startColor = graphic.color.SetA(startAlpha);
            Color endColor = graphic.color.SetA(endAlpha);
            float progress = 0.0f;

            while (progress < 1.0f)
            {
                progress += Time.deltaTime / duration;
                graphic.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }

            graphic.color = endColor;
        }
    }
}
