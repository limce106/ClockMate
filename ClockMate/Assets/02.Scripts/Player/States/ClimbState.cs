using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    public readonly ClimbObjectBase climbTarget;

    private const float climbSpeed = 3f;

    private Rigidbody _rb;
    private RigidbodyConstraints _originalConstraints;

    public ClimbState(CharacterBase character, ClimbObjectBase climbTarget)
    {
        _character = character;
        this.climbTarget = climbTarget;
    }

    public void Enter()
    {
        _rb = _character.GetComponent<Rigidbody>();
        StartClimbing();
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

    void StartClimbing()
    {
        _originalConstraints = _rb.constraints;

        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        climbTarget.AttachTo(_character);
    }


    public void Climb(float vertical)
    {
        _rb.velocity = new Vector3(0f, vertical * climbSpeed, 0f);
    }

    public void StopClimbing()
    {
        _rb.useGravity = true;
        _rb.constraints = _originalConstraints;

        _character.ChangeState<IdleState>();
        climbTarget.CloseUI();
    }
}
