using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ö� �� �ִ� ������Ʈ
/// Layer�� Climable ���� �ʿ�
/// </summary>

public class IAClimbObject : MonoBehaviourPun, IInteractable
{
    private UIManager _uiManager;
    private UINotice _uiNotice;
    private UIClimbableObj _uiClimbableObj;

    private Sprite _climbSprite;
    private string _climbString;

    public Vector3 attachOffset = new Vector3(0f, 0.3f, 0f);

    private bool _isColliding = false;
    private const float climbTopOffset = 0.05f;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _uiManager = UIManager.Instance;
    }

    public bool CanInteract(CharacterBase character)
    {
        if (!_isColliding)
            return false;

        if (character.CurrentState is ClimbState)
            return false;

        if(!character.IsGrounded)
            return false;

        // �÷��̾ ������Ʈ ���� ������ ��ȣ�ۿ� �Ұ�
        Collider collider = GetComponent<Collider>();
        float climbTopY = collider.bounds.max.y;
        float playerY = character.transform.position.y;

        if (playerY >= climbTopY - climbTopOffset)
            return false;

        return true;
    }

    public void OnInteractAvailable()
    {
        _uiNotice = _uiManager.Show<UINotice>("UINotice");

        _climbSprite = Resources.Load<Sprite>("UI/Sprites/interact_active");
        _climbString = "���ö󰡱�";

        _uiNotice.SetImage(_climbSprite);
        _uiNotice.SetText(_climbString);
    }

    public void OnInteractUnavailable()
    {
        _uiManager.Close(_uiNotice);
    }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<ClimbState>(this, attachOffset);

        _uiClimbableObj = _uiManager.Show<UIClimbableObj>("UIClimbableObj");

        _climbSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _climbString = "���ö󰡱� ����";

        _uiNotice.SetImage(_climbSprite);
        _uiNotice.SetText(_climbString);

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        // �������� �߿� �ݸ����� ����� �ִ� ��ġ�� �ٴٸ� ������ �ν�
        if(collision.collider.IsPlayerCollider())
        {
            CharacterBase character = collision.gameObject.GetComponent<CharacterBase>();
            ClimbState climbState = character.CurrentState as ClimbState;
            _isColliding = false;

            if (climbState != null)
            {
                climbState.StopClimbing();
                character.ChangeState<IdleState>();
                climbState.climbTarget.CloseUI();
            }
        }
    }

    /// <summary>
    /// ����Ű UI ���ֱ�
    /// </summary>
    public void CloseUI()
    {
        _uiManager.Close(_uiClimbableObj);
        _uiManager.Close(_uiNotice);
    }
}
