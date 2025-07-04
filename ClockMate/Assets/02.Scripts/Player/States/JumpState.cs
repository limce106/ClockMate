using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IState
{
    private readonly CharacterBase _character;
    private float _waitTime; // 땅 감지 너무 빠르게 하지 않게 

    public JumpState(CharacterBase character) => _character = character;
    public void Enter()
    {
        _waitTime = 0f;
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        _waitTime += Time.deltaTime;
        // 캐릭터가 땅에 닿으면 다시 Idle 상태로 전환
        if (_waitTime > 0.1f && _character.IsGrounded)
        {
            _character.ChangeState<IdleState>();
        }
    }

    public void Exit()
    {
        
    }
}
