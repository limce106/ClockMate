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
        _player.TryJump();
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
