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

    private void Awake()
    {
        UIType = UI.UIType.Windowed;
        _filled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Filled");
        _unfilled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Unfilled");
        if (_filled == null) Debug.LogWarning("[UITurretActive] filled sprite not found]");
        if (_unfilled == null) Debug.LogWarning("[UITurretActive] unfilled sprite not found]");
    }

    public override void Show()
    {
        base.Show();
        var helpUI = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        helpUI.SetControl(Key.Arrows.LoadSprite(), "조준하기", Key.Space.LoadSprite(Style.Outline), "발사하기");
        cbClose += helpUI.Close;
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
