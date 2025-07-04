using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : IState
{
    
    private readonly CharacterBase _character;

    public DeadState(CharacterBase character) =>_character = character;
    
    public void Enter()
    {
        _character.gameObject.SetActive(false);
        StageLifeManager.Instance.HandleDeath(_character);
        
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        _character.gameObject.SetActive(true);
    }

}
