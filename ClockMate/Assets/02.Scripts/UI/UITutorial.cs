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

    private Sprite _tutorialSprite;
    private bool _isControlTutorial = true;

    private void Awake()
    {
        _tutorialSprite = GetComponent<Image>().sprite;
    }

    public override void Initialize(object data)
    {
        if(data is bool isControlTutorial)
        {
            _isControlTutorial = isControlTutorial;
        }
    }

    public override void Show()
    {
        GameManager.Instance.GetLocalCharacter().enabled = false;

        if( _isControlTutorial)
        {
            _tutorialSprite = _control1Img;
        }
        else
        {
            _tutorialSprite = _buttonImg;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.GetLocalCharacter().enabled = true;
    }

    public void OnClick_Ok()
    {
        if(_tutorialSprite == _control2Img || _tutorialSprite == _buttonImg)
        {
            UIManager.Instance.Close(this);
        }
        else
        {
            _tutorialSprite = _control2Img;
        }
    }
}
