public class WalkState : IState
{
    private readonly CharacterBase _character;
    
    public WalkState(CharacterBase character) => _character = character;
    public void Enter()
    {
        // 애니메이션 시작
    }

    public void Update()
    {
        
    }

    public void FixedUpdate()
    {
        _character.Move();
    }

    public void Exit()
    {
        // 애니메이션 멈춤
    }
}