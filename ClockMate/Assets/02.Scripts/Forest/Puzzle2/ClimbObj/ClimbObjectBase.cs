using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClimbObjectBase : MonoBehaviour, IInteractable
{
    protected UIManager _uiManager;
    protected UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;
    protected UIClimbableObj _uiClimbableObj;

    protected bool _isColliding = false;
    public bool isInteractFromDown { private set; get; } = false; // 아래에서 상호작용 했는지

    [SerializeField] protected Transform topPoint;
    [SerializeField] protected Transform bottomPoint;

    public float topY { private set; get; }
    public float bottomY { private set; get; }

    protected virtual void Awake()
    {
        _uiManager = UIManager.Instance;

        topY = topPoint.position.y;
        bottomY = bottomPoint.position.y;

        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "그만타기";
    }

    /// <summary>
    /// 조작키 UI 없애기
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

        if (_isColliding)
            return true;
        else
            return false;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<ClimbState>(this);

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        _uiClimbableObj = _uiManager.Show<UIClimbableObj>("UIClimbableObj");

        EnableColliders(false);

        return true;
    }

    public void EnableColliders(bool enable)
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = enable;
    }

    public abstract void AttachTo(CharacterBase character);

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isColliding = false;
    }

    protected void SetIsInteractFromDown(bool fromDown)
    {
        isInteractFromDown = fromDown;
    }
}
