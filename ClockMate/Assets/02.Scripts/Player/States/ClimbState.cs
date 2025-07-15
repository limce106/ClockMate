using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    private readonly Transform _climbTarget;
    private readonly Vector3 _climbOffset;

    public float climbSpeed = 1f;

    private Rigidbody _rb;

    public ClimbState(CharacterBase character, Transform climbTarget, Vector3 offset)
    {
        _character = character;
        _climbTarget = climbTarget;
        _climbOffset = offset;
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
        float vertical = Input.GetAxisRaw("Vertical");
        _rb.velocity = new Vector3(0f, vertical * climbSpeed, 0f);

        if(Input.GetKeyDown(KeyCode.E))
        {
            StopClimbing();
        }
    }

    public void Exit()
    {
        StopClimbing();
    }

    void StartClimbing()
    {
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;

        // 벽에 붙이기
        if(_climbTarget != null)
        {
            Vector3 dirToWall = (_climbTarget.position - _character.transform.position).normalized;
            Vector3 horizontalDir = new Vector3(dirToWall.x, 0f, dirToWall.z).normalized;

            Vector3 attachPos = _climbTarget.position + (horizontalDir * _climbOffset.z) + new Vector3(0f, _climbOffset.y, 0f);
            _character.transform.position = attachPos;
            _character.transform.forward = -horizontalDir;
        }
    }

    void StopClimbing()
    {
        _rb.useGravity = true;
    }
}
