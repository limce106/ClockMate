using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IState
{
    private readonly PlayerBase _player;

    public JumpState(PlayerBase player)
    {
        _player = player;
    }
    public void Enter()
    {
        if (!_player.TryJump())
        {
            _player.ChangeState(_player.IdleState); // 점프 실패 시 Idle 상태로 전환
        }
    }

    public void Update()
    {
        if (_player.IsGrounded)
        {
            _player.ChangeState(_player.IdleState);
        }
    }

    public void Exit()
    {
        
    }
}
