using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MStateDead : IMonsterState
{
    private readonly MonsterController _monster;


    public MStateDead(MonsterController monster) => _monster = monster;
    public void Enter()
    {
        _monster.Die();
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }
}
