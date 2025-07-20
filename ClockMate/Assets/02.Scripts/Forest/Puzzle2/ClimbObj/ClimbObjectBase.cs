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

    [SerializeField] protected Transform topPoint;
    [SerializeField] protected Transform bottomPoint;

    [Tooltip("�ν����Ϳ��� �� ������ �ʿ����")]
    public float topY;
    public float bottomY;

    protected virtual void Awake()
    {
        _uiManager = UIManager.Instance;

        topY = topPoint.position.y;
        bottomY = bottomPoint.position.y;
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
    /// ����Ű UI ���ֱ�
    /// </summary>
    public void CloseUI()
    {
        _uiManager.Close(_uiClimbableObj);
        _uiManager.Close(_uiNotice);
    }

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
        ShowNoticeUI("UI/Sprites/interact_active", "���ö󰡱�");
    }

    public void OnInteractUnavailable()
    {
        _uiManager.Close(_uiNotice);
    }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<ClimbState>(this);

        _uiClimbableObj = _uiManager.Show<UIClimbableObj>("UIClimbableObj");
        ShowNoticeUI("UI/Sprites/keyboard_q_outline", "���ö󰡱� ����");

        return true;
    }

    public abstract void AttachTo(CharacterBase character);
}
