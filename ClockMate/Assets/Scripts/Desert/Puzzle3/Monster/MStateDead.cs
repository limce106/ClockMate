using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MStateDead : IMonsterState
{
    private readonly MonsterController _monster;


    public MStateDead(MonsterController monster) => _monster = monster;
    public void Enter()
    {
        _monster.gameObject.SetActive(false);
        // TODO 사망 처리, 키 드롭 
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }
}
