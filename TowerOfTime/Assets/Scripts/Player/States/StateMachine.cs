public class StateMachine
{
    public IState CurrentState {get; private set;}

    public void ChangeStateTo(IState newState)
    {
        if (CurrentState == newState || !CanTransition(CurrentState, newState)) return;
        
        CurrentState?.Exit(); // 이전 상태 정리
        CurrentState = newState; // 새 상태로 교체
        CurrentState?.Enter(); // 새 상태 준비
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
    
    private bool CanTransition(IState from, IState to)
    {
        // Move는 Idle에서만 가능
        if (!(from is IdleState) && to is WalkState)
            return false;

        return true;
    }
}
