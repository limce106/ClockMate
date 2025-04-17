using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState {get; private set;}

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit(); // 이전 상태 정리
        CurrentState = newState; // 새 상태로 교체
        CurrentState?.Enter(); // 새 상태 준비
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}
