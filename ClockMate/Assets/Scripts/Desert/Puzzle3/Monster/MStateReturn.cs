
public class MStateReturn : IMonsterState
{
    private readonly MonsterController _monster;
    
    public MStateReturn(MonsterController monster) => _monster = monster;
    
    public void Enter()
    {
        _monster.StopChaseAndReturn();
    }

    public void Update()
    {
        if (_monster.CanSeeHour())
        {
            _monster.ChangeStateTo<MStateChase>();
            return;
        }

        if (_monster.IsReturnComplete())
        {
            _monster.ChangeStateTo<MStatePatrol>();
        }
    }

    public void Exit()
    {
        
    }
}
