using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private readonly PlayerBase _player;

    public IdleState(PlayerBase player)
    {
        _player = player;
    }
    public void Enter()
    {
        
    }

    public void Update()
    {
        Vector3 dir = _player.Input.GetDirectionRelativeTo(Camera.main.transform);
        _player.Move(dir);

        if (_player.Input.JumpPressed)
        {
            _player.ChangeState(_player.JumpState);
        }
        else if (_player.Input.InteractPressed)
        {
            _player.OnInteract();
        }
    }

    public void Exit()
    {
        
    }
}
