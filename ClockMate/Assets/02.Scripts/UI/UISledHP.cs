using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISledHP : UIBase {
    [SerializeField] private Image imgHpBar;

    public void UpdateHpBar(int maxHP, int currentHP)
    {
        imgHpBar.fillAmount = (float) currentHP / maxHP;
    }
}
