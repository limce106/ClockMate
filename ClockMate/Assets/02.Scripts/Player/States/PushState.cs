using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PushState : IState
{
    private readonly CharacterBase _character;
    private Quaternion _followRotation;
    
    public PushState(CharacterBase character, Quaternion followRotation)
    {
        _character = character;
        _followRotation = followRotation;
    } 
    public void Enter()
    {
        _character.transform.rotation = _followRotation;

    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }
}
