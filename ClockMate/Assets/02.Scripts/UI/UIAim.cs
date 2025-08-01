using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAim : UIBase
{
    [field: SerializeField] public RectTransform AimTransform { get; private set; }
    [SerializeField] private Image aimImg;
    [SerializeField] private float aimSpeed;

    private Sprite _imgDefault;
    private Sprite _imgDetected;
    
    private readonly float _screenHalfWidth = 1920f / 2;
    private readonly float _screenHalfHeight = 1080f / 2;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _imgDefault = Resources.Load<Sprite>("UI/Sprites/AimDefault");
        _imgDetected = Resources.Load<Sprite>("UI/Sprites/AimDetected");
        aimImg.sprite = _imgDefault;
    }

    private void Update()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        AimTransform.anchoredPosition += move * (aimSpeed * Time.deltaTime);

        // 화면 밖으로 나가지 않게 클램프 처리
        Vector2 clampedPos = AimTransform.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -_screenHalfWidth, _screenHalfWidth);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -_screenHalfHeight, _screenHalfHeight);
        AimTransform.anchoredPosition = clampedPos;
    }
    
    public void UpdateImage(bool isTargetDetected)
    {
        aimImg.sprite = isTargetDetected ? _imgDetected : _imgDefault;
    }
}
