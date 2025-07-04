using UnityEngine;

public class MStateChase : IMonsterState
{
    private readonly MonsterController _monster;
    private float _lostSightTimer;
    private const float LostSightDuration = 5.0f; // 시야 상실 후 추적 유지 시간


    public MStateChase(MonsterController monster) => _monster = monster;
    public void Enter()
    {
        _lostSightTimer = 0f; // 타이머 리셋
    }

    public void Update()
    {
        _monster.ChaseHour();

        if (_monster.CanSeeHour())
        {
            // 아워가 시야 범위 내라면 타이머 리셋
            _lostSightTimer = 0f; 
        }
        else
        {
            _lostSightTimer += Time.deltaTime; 
            if (_lostSightTimer >= LostSightDuration)
            {
                _monster.ChangeStateTo<MStateReturn>();
            }
        }
    }

    public void Exit()
    {
        
    }
}
