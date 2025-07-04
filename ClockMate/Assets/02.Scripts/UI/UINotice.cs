using Define;
using UnityEngine;
using UnityEngine.UI;

public class UINotice : UIBase
{
    [SerializeField] private Image img;
    [SerializeField] private Text text;
    
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
}
