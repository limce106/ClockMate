using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurretAcive : UIBase
{
    [SerializeField] private Image[] imgChargeLv;

    public void Reset()
    {
        foreach (Image image in imgChargeLv)
        {
            image.color = Color.black;
        }
    }
    
    public void UpdateChargeImg(int chargeLv)
    {
        for (int i = 0; i < imgChargeLv.Length; i++)
        {
            imgChargeLv[i].color = i < chargeLv ? Color.green : Color.black;
        }
    }
}
