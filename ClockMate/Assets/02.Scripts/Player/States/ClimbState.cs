using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    private readonly Transform _climbTarget;
    private readonly Vector3 _attachOffset;

    private const float climbSpeed = 3f;

    private Rigidbody _rb;
    private RigidbodyConstraints _originalConstraints;

    private const float ForwardCheckOffset = 0.3f;
    private const float RayDistance = 0.5f;

    public ClimbState(CharacterBase character, Transform climbTarget, Vector3 attachOffset)
    {
        _character = character;
        _climbTarget = climbTarget;
        _attachOffset = attachOffset;
    }

    public void Enter()
    {
        _rb = _character.GetComponent<Rigidbody>();
        StartClimbing();

        _rb.useGravity = false;
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

        // 오브젝트와의 가장 가까운 지점에 붙이기
        if (_climbTarget != null)
        {
            Vector3 climbPoint = _climbTarget.GetComponent<Collider>().ClosestPoint(_character.transform.position) + _attachOffset;
            _character.transform.position = climbPoint;

            Vector3 dirToCenter = (_climbTarget.transform.position - _character.transform.position).normalized;
            dirToCenter.y = 0;
            _character.transform.forward = dirToCenter;
        }
    }

    public void Climb(float vertical)
    {
        _rb.velocity = new Vector3(0f, vertical * climbSpeed, 0f);
    }

    public void StopClimbing()
    {
        _rb.useGravity = true;
        _rb.constraints = _originalConstraints;
    }
}
