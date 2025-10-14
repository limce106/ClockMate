using UnityEngine;
using UnityEngine.UI;

public class UITurretActive : UIBase
{
    [SerializeField] private Image[] imgChargeLv;
    private Sprite _filled;
    private Sprite _unfilled;

    private void Awake()
    {
        _filled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Filled");
        _unfilled = Resources.Load<Sprite>("UI/Sprites/Icon_Battery_Unfilled");
        if (_filled == null) Debug.LogWarning("[UITurretActive] filled sprite not found]");
        if (_unfilled == null) Debug.LogWarning("[UITurretActive] unfilled sprite not found]");
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
