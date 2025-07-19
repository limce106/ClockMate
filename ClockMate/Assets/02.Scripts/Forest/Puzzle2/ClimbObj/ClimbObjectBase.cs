using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClimbObjectBase : MonoBehaviour, IClimableObject
{
    protected UIManager _uiManager;
    protected UINotice _uiNotice;
    protected UIClimbableObj _uiClimbableObj;

    protected Sprite _climbSprite;
    protected string _climbString;

    public Vector3 attachOffset = new Vector3(0f, 0.3f, 0f);

    public float topY;
    public float bottomY;

    protected virtual void Awake()
    {
        _uiManager = UIManager.Instance;
    }

    protected void ShowNoticeUI(string spritePath, string text)
    {
        if (_uiNotice == null || !_uiManager.IsOnScreen(_uiNotice))
        {
            _uiNotice = _uiManager.Show<UINotice>("UINotice");
        }

        _climbSprite = Resources.Load<Sprite>(spritePath);
        _climbString = text;

        _uiNotice.SetImage(_climbSprite);
        _uiNotice.SetText(_climbString);
    }

    /// <summary>
    /// 조작키 UI 없애기
    /// </summary>
    public void CloseUI()
    {
        _uiManager.Close(_uiClimbableObj);
        _uiManager.Close(_uiNotice);
    }

    //protected void OnCollisionExit(Collision collision)
    //{
    //    // 기어오르는 중에 콜리전을 벗어나면 최대 위치에 다다른 것으로 인식
    //    if (collision.collider.IsPlayerCollider())
    //    {
    //        CharacterBase character = collision.gameObject.GetComponent<CharacterBase>();
    //        ClimbState climbState = character.CurrentState as ClimbState;

    //        if (climbState != null)
    //        {
    //            climbState.StopClimbing();
    //        }
    //    }
    //}

    public virtual bool CanInteract(CharacterBase character)
    {
        if (character.CurrentState is ClimbState)
            return false;

        if (!character.IsGrounded)
            return false;

        return true;
    }

    public void OnInteractAvailable()
    {
        ShowNoticeUI("UI/Sprites/interact_active", "기어올라가기");
    }

    public void OnInteractUnavailable()
    {
        _uiManager.Close(_uiNotice);
    }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<ClimbState>(this);

        _uiClimbableObj = _uiManager.Show<UIClimbableObj>("UIClimbableObj");
        ShowNoticeUI("UI/Sprites/keyboard_q_outline", "기어올라가기 종료");

        return true;
    }

    public abstract void AttachTo(CharacterBase character);
}
