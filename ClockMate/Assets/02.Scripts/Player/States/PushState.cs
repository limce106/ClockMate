using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushState : IState
{
    private readonly CharacterBase _character;
    private Transform _followTransform;
    
    public PushState(CharacterBase character, Transform followTransform)
    {
        _character = character;
        _followTransform = followTransform;
    } 
    public void Enter()
    {
        _character.transform.rotation = Quaternion.LookRotation(_followTransform.forward);
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
