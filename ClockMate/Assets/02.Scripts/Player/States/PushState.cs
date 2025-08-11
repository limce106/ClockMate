using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if(SceneManager.GetActiveScene().ToString() == "Glacier")
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
