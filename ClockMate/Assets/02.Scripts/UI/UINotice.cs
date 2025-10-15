using Define;
using UnityEngine;
using UnityEngine.UI;

public class UINotice : UIBase
{
    [SerializeField] private Image img;
    [SerializeField] private Text text;
    
    [SerializeField] private RectTransform[] rectTransforms;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        UIType = UI.UIType.Windowed;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetImage(Sprite sprite)
    {
        img.sprite = sprite;
    }

    public void SetImageActive(bool value)
    {
        img.enabled = value;
    }

    public void SetVerticalPos(bool isTop)
    {
        float yPos = isTop ? -100f : -980f;
        foreach (RectTransform rt in rectTransforms)
        {
            Vector2 ap = rt.anchoredPosition;
            ap.y = yPos;
            rt.anchoredPosition = ap;
        }
    }

    private void OnDisable()
    {
        SetVerticalPos(true);
    }
}
