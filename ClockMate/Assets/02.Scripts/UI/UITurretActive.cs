using System;
using Define;
using DefineExtension;
using UnityEngine;
using UnityEngine.UI;
using static Define.Icon;

public class UITurretActive : UIBase
{
    [SerializeField] private Image[] imgChargeLv;
    private Sprite _filled;
    private Sprite _unfilled;

    private UINotice _uiNotice;
    private UIControlHelp _uiHelp;
    private Sprite _dropSprite;
    private string _dropString;
    
    private void Awake()
    {
        UIType = UI.UIType.Windowed;
        _dropSprite = Key.Q.LoadSprite(Style.Outline);;
        _dropString = "나가기";
        _filled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Filled");
        _unfilled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Unfilled");
        if (_filled == null) Debug.LogWarning("[UITurretActive] filled sprite not found]");
        if (_unfilled == null) Debug.LogWarning("[UITurretActive] unfilled sprite not found]");
    }

    public override void Show()
    {
        base.Show();
        // 터렛 조작 그만두기 UI 표시
        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_dropSprite);
        _uiNotice.SetText(_dropString);
        _uiHelp = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        _uiHelp.SetControl(Key.Arrows.LoadSprite(), "조준하기", Key.Space.LoadSprite(Style.Outline), "발사하기");
        
        cbClose -= _uiHelp.Close;
        cbClose += _uiHelp.Close;
        cbClose -= _uiNotice.Close;
        cbClose += _uiNotice.Close;
    }

    public void Reset()
    {
        foreach (Image image in imgChargeLv)
        {
            image.sprite = _unfilled;
        }
    }
    
    public void UpdateChargeImg(int chargeLv)
    {
        for (int i = 0; i < imgChargeLv.Length; i++)
        {
            imgChargeLv[i].sprite = i < chargeLv ? _filled : _unfilled;
        }
    }
}
