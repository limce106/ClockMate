using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurretAcive : UIBase
{
    [SerializeField] private Image[] imgChargeLv;
    private int _currentChargeIndex;

    public void Reset()
    {
        foreach (Image image in imgChargeLv)
        {
            image.color = Color.black;
        }
        _currentChargeIndex = 0;
    }

    public void SetInitialChargeLv(int lv)
    {
        for (int i = 0; i < lv; i++)
        {
            imgChargeLv[i].color = Color.green;
        }

        _currentChargeIndex = lv - 1;
    }
    public void UpdateChargeImg(bool isCharged)
    {
        if (isCharged)
        {
            imgChargeLv[_currentChargeIndex].color = Color.green;
            _currentChargeIndex++;
        }
        else
        {
            imgChargeLv[_currentChargeIndex].color = Color.black;   
            _currentChargeIndex--;
        }
    }
}
