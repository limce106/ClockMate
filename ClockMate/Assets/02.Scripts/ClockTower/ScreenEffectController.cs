using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ScreenEffectController : MonoBehaviour
{
    public Image fadeImg;
    public Volume volume;

    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;

    // 따뜻함 상태
    private float warmthLevel = 0f;
    private float warmthStep = maxWarmth/ 3;
    private const float maxWarmth = 0.6f;

    void Start()
    {
        if(!volume.profile.TryGet(out colorAdjustments))
            Debug.LogError("ColorAdjustments not found in Volume Profile!");
        if (!volume.profile.TryGet(out whiteBalance))
            Debug.LogError("WhiteBalance not found in Volume Profile!");
    }

    public IEnumerator EnableGrayscale(bool isGrayscale)
    {
        if(isGrayscale)
            yield return StartCoroutine(LerpEffect(-100f, colorAdjustments.postExposure.value, whiteBalance.temperature.value, 0.5f));
        else
            colorAdjustments.saturation.value = 0f;
    }

    public void IncreaseWarmth()
    {
        warmthLevel += warmthStep;
        warmthLevel = Mathf.Clamp01(warmthLevel);

        float targetSaturation = Mathf.Lerp(0f, 40f, warmthLevel);      // 생동감
        float targetExposure = Mathf.Lerp(0f, maxWarmth, warmthLevel);  // 밝기
        float targetTemperature = Mathf.Lerp(0f, 20f, warmthLevel);     //노란기

        StartCoroutine(LerpEffect(targetSaturation, targetExposure, targetTemperature, 3f));
    }

    private IEnumerator LerpEffect(float targetSaturation, float targetExposure, float targetTemperature, float transitionTime)
    {
        float t = 0f;

        float startSaturation = colorAdjustments.saturation.value;
        float startExposure = colorAdjustments.postExposure.value;
        float startTemperature = whiteBalance.temperature.value;

        while(t  < 1f)
        {
            t += Time.deltaTime / transitionTime;

            colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, targetSaturation, t);
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, t);
            whiteBalance.temperature.value = Mathf.Lerp(startTemperature, targetTemperature, t);

            yield return null;
        }
    }

    public IEnumerator FadeIn(float duration)
    {
        float time = 0f;
        Color color = fadeImg.color;

        while(time < duration)
        {
            color.a = Mathf.Lerp(1f, 0f, time/duration);
            fadeImg.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        fadeImg.color = color;
    }

    public IEnumerator FadeOut(float duration)
    {
        float time = 0f;
        Color color = fadeImg.color;

        while (time < duration)
        {
            color.a = Mathf.Lerp(0f, 1f, time / duration);
            fadeImg.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        fadeImg.color = color;
    }
}
