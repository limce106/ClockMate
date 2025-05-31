public class MStatePatrol : IMonsterState
{
    private readonly MonsterController _monster;
    private int _currentIndex;
    
    public MStatePatrol(MonsterController monster) => _monster = monster;
    
    public void Enter()
    {
        _monster.Agent.SetDestination(_monster.PatrolPoints[_currentIndex].position);
    }

    public void Update()
    {
        if (_monster.CanSeeHour())
        {
            _monster.ChangeStateTo<MStateChase>();
            return;
        }

        if (!_monster.Agent.pathPending && _monster.Agent.remainingDistance < 0.5f)
        {
            _currentIndex = (_currentIndex + 1) % _monster.PatrolPoints.Length;
            _monster.Agent.SetDestination(_monster.PatrolPoints[_currentIndex].position);
        }
    }

    public void Exit()
    {
        
    }
}
