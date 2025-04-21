using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IState
{
    private readonly CharacterBase _character;

    public JumpState(CharacterBase character) => _character = character;
    public void Enter()
    {
        if (!_character.TryJump())
        {
            _character.TryChangeState(_character.IdleState); // 점프 실패 시 Idle 상태로 전환
        }
    }

    public void FixedUpdate()
    {
        _character.Move();
    }

    public void Update()
    {
        // 캐릭터가 땅에 닿으면 다시 Idle 상태로 전환
        if (_character.IsGrounded)
        {
            _character.TryChangeState(_character.IdleState);
        }
    }

    public void Exit()
    {
        
    }
}
