using System;
using System.Collections;
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

    private Dictionary<CharacterAction, bool> _actionsAvailable;

    private void Awake()
    {
        _character = GetComponent<CharacterBase>();
        _inputActions = new PlayerInputActions();
        _isMoving = false;

        _actionsAvailable = new Dictionary<CharacterAction, bool>
        {
            { CharacterAction.Move, true },
            { CharacterAction.Jump, true }
        };

        InitInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
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

        Vector2 input = _inputActions.Player.Move.ReadValue<Vector2>();
        _character.Move(new Vector3(input.x, 0, input.y));
        if (input.sqrMagnitude < 0.01f)
        {
            _isMoving = false;
            _character.ChangeState(_character.IdleState);
        }
    }

    private void OnMovePressed(InputAction.CallbackContext context)
    {
        if (!_actionsAvailable[CharacterAction.Move]) return;
        _isMoving = true;
        _character.ChangeState(_character.WalkState);
    }


    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (!_actionsAvailable[CharacterAction.Jump] || !_character.CanJump()) return;
        _character.ChangeState(_character.JumpState);
        _character.PerformJump();
    }
    
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        //_character.ChangeState(_character.InteractState);
    }
    
    private void OnAbilityPressed(InputAction.CallbackContext context)
    {
        //_character.ChangeState(_character.AbilityState);
    }

    public void ActivateInputActions(HashSet<CharacterAction> availableActions)
    {
        foreach (CharacterAction action in _actionsAvailable.Keys)
        {
            _actionsAvailable[action] = availableActions.Contains(action);
        }
    }
    
}
