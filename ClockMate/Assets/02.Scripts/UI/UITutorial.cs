using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITutorial : UIBase
{
    [SerializeField] private Image _tutorial;

    [SerializeField] private Sprite _control1Img;
    [SerializeField] private Sprite _control2Img;
    [SerializeField] private Sprite _buttonImg;

    private bool _isControlTutorial = true;

    public override void Initialize(object data)
    {
        if (data is bool isControlTutorial)
        {
            _isControlTutorial = isControlTutorial;
        }
    }

    public override void Show()
    {
        Time.timeScale = 0f;

        if( _isControlTutorial)
        {
            _tutorial.sprite = _control1Img;
        }
        else
        {
            _tutorial.sprite = _buttonImg;
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void OnClick_Ok()
    {
        if(_tutorial.sprite == _control2Img || _tutorial.sprite == _buttonImg)
        {
            UIManager.Instance.Close(this);
        }
        else
        {
            _tutorial.sprite = _control2Img;
        }

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
}
