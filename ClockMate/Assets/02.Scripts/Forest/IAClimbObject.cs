using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기어올라갈 수 있는 오브젝트
/// Layer는 Climable 지정 필요
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

        // 플레이어가 오브젝트 위에 있으면 상호작용 불가
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
        _climbString = "기어올라가기";

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
        _climbString = "기어올라가기 종료";

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
        // 기어오르는 중에 콜리전을 벗어나면 최대 위치에 다다른 것으로 인식
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
    /// 조작키 UI 없애기
    /// </summary>
    public void CloseUI()
    {
        _uiManager.Close(_uiClimbableObj);
        _uiManager.Close(_uiNotice);
    }
}
