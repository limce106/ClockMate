using UnityEngine;
using UnityEngine.UI;

public class TimelineUIBinder : MonoBehaviour
{
    [Header("Image Slots")]
    public Image imageA;
    public Image imageB;

    [Header("Text")]
    public Text targetText;

    [Header("Optional Hide/Show Root")]
    public CanvasGroup rootCanvasGroup; // 전체 숨김용

    // 편하게 RectTransform 캐싱
    public RectTransform RectA => imageA ? imageA.rectTransform : null;
    public RectTransform RectB => imageB ? imageB.rectTransform : null;
}
