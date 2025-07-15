using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    private readonly Transform _climbTarget;
    private readonly float _yOffset;

    public float climbSpeed = 1f;

    private Rigidbody _rb;
    private RigidbodyConstraints _originalConstraints;

    private const float ForwardCheckOffset = 0.3f;
    private const float RayDistance = 0.5f;

    public ClimbState(CharacterBase character, Transform climbTarget, float yOffset)
    {
        _character = character;
        _climbTarget = climbTarget;
        _yOffset = yOffset;
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
        if (!IsClimbObjAbove())
        {
            StopClimbing();
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            Climb(verticalInput);
        }
        else if (Input.GetKeyDown(KeyCode.E))
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
        _originalConstraints = _rb.constraints;

        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        // 오브젝트와의 가장 가까운 지점에 붙이기
        if (_climbTarget != null)
        {
            Vector3 climbPoint = _climbTarget.GetComponent<Collider>().ClosestPoint(_character.transform.position);
            climbPoint.y += _yOffset;
            Vector3 dirToWall = (climbPoint - _character.transform.position).normalized;
            Vector3 horizontalDir = new Vector3(dirToWall.x, 0f, dirToWall.z).normalized;

            _character.transform.position = climbPoint;
            _character.transform.forward = horizontalDir;
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

    /// <summary>
    /// 올라가고 있는 오브젝트의 끝지점인지 반환
    /// </summary>
    private bool IsClimbObjAbove()
    {
        Vector3 origin = _character.transform.position + Vector3.up + _character.transform.forward * ForwardCheckOffset;
        Vector3 direction = _character.transform.forward;

        return Physics.Raycast(origin, direction, RayDistance, LayerMask.GetMask("Climbable"));
    }
}
