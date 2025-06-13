using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;
using InputAction = UnityEngine.InputSystem.InputAction;

/// <summary>
/// 사용자 입력을 감지하는 입력 처리기.
/// Unity Input System 기반
/// </summary>
[RequireComponent(typeof(CharacterBase))]
public class PlayerInputHandler : MonoBehaviour
{
    private CharacterBase _character;
    private PlayerInputActions _inputActions;
    private bool _isMoving;
    private Camera _camera;

    private Dictionary<CharacterAction, bool> _actionsAvailable;

    private void Awake()
    {
        Init();

        PhotonView photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine)
        {
            enabled = false;
        }      
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Init()
    {
        _character = GetComponent<CharacterBase>();
        _inputActions = new PlayerInputActions();
        _isMoving = false;
        _camera = Camera.main;

        _actionsAvailable = new Dictionary<CharacterAction, bool>
        {
            { CharacterAction.Move, true },
            { CharacterAction.Jump, true },
            { CharacterAction.Interact, true },
        };
        
        InitInputActions();
    }

    private void InitInputActions()
    {
        _inputActions.Player.Jump.performed += OnJumpPressed;
        _inputActions.Player.Interact.performed += OnInteractPressed;
        _inputActions.Player.Ability.performed += OnAbilityPressed;
        _inputActions.Player.Move.performed += OnMovePressed;
    }

    private void FixedUpdate()
    {
        if (!_isMoving) return;

        HandleMovement();
    }

    /// <summary>
    /// 사용자 입력을 읽고 카메라 기준으로 이동 벡터를 계산한 후 캐릭터 이동을 처리한다.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputDirection = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
        
        // 카메라 기준 이동 방향 벡터 (y축 제거 후 정규화)
        Vector3 camForward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
        
        // 카메라(화면) 기준 이동 방향 계산
        Vector3 moveDirection = (camForward * inputDirection.y) + (camRight * inputDirection.x);
        
        // 캐릭터 이동 처리
        _character.Move(moveDirection);
        
        // 입력이 거의 없으면 이동 상태 종료 처리
        if (!(inputDirection.sqrMagnitude < 0.01f)) return;
        _isMoving = false;
        _character.ChangeState<IdleState>();
    }

    private void OnMovePressed(InputAction.CallbackContext context)
    {
        if (!_actionsAvailable[CharacterAction.Move]) return;
        _isMoving = true;
        _character.ChangeState<WalkState>();
    }


    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!_actionsAvailable[CharacterAction.Jump] || !_character.CanJump()) return;
        _character.ChangeState<JumpState>();
        _character.PerformJump();
    }
    
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (!_actionsAvailable[CharacterAction.Interact]) return;
        //_character.ChangeState<InteractState>();
        _character.InteractionDetector.TryInteract();
    }
    
    private void OnAbilityPressed(InputAction.CallbackContext context)
    {
        //_character.ChangeState<AbilityState>();
    }

    public void SetInputActionsActive(List<CharacterAction> actions, bool value)
    {
        foreach (CharacterAction action in actions)
        {
            _actionsAvailable[action] = value;
        }
    }
}
