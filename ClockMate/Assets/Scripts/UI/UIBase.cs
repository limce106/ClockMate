using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Define;

/// <summary>
/// 모든 UI 오브젝트가 상속하는 기본 클래스.
/// Show, Close, ESC 대응(BackKey) 메소드 제공.
/// </summary>
public abstract class UIBase : MonoBehaviour
{
    [field: SerializeField]
    public UI.UIType UIType { get; set; } // FullScreen 여부 등 UI 타입 설정

    protected Action cbClose; // UI 닫힐 때 호출할 콜백

    /// <summary>
    /// UI가 켜지는 시점에 호출되는 메소드.
    /// </summary>
    public virtual void Show()
    {
        
    }

    /// <summary>
    /// ESC 입력 시 처리 로직. true 반환 시 UI 닫힘.
    /// </summary>
    public virtual bool BackKey()
    {
        return false; // 기본은 esc 눌러도 닫히지 x
    }

    /// <summary>
    /// UI를 끄는(닫는) 시점에 호출되는 메소드
    /// </summary>
    public void Close()
    {
        if (UIManager.Instance.Close(this))
        {
            cbClose?.Invoke(); // 콜백 메소드가 존재하면 실행
            cbClose = null;
        }
    }
}