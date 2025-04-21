using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 사용자 입력을 감지하고 PlayerBase로 전달하는 입력 처리기.
/// Unity Input System 기반
/// </summary>
[RequireComponent(typeof(CharacterBase))]
public class PlayerInputHandler : MonoBehaviour
{
    private CharacterBase _character;
    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _character = GetComponent<CharacterBase>();
        _inputActions = new PlayerInputActions();
        
        InitInputActions();
    }

    void Update()
    {
        HandleMoveInput();
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
    }

    private void HandleMoveInput()
    {
        Vector2 input = _inputActions.Player.Move.ReadValue<Vector2>();
        _character.UpdateMoveDirection(new Vector3(input.x, 0, input.y));
        _character.TryChangeState(_character.MoveState);
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        _character.TryChangeState(_character.JumpState);
    }
    
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        //_character.TryChangeState(_character.InteractState);
    }
    
    private void OnAbilityPressed(InputAction.CallbackContext context)
    {
        //_character.TryChangeState(_character.AbilityState);
    }
    
}
