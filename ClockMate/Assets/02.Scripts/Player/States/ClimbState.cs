using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    public readonly IAClimbObject climbTarget;
    private readonly Vector3 _attachOffset;

    private const float climbSpeed = 3f;
    private const float surfaceOffset = 0.1f;   // ǥ�鿡�� ������ �Ÿ�
    private const float rayOriginBackOffset = 0.1f;   // �ʹ� ����� ��ġ���� Raycast�� ��� ���� �浹���� �ʰų� ���ο��� ���۵ǹǷ� ��¦ �ڷ� �������� ����

    private Rigidbody _rb;
    private RigidbodyConstraints _originalConstraints;

    public ClimbState(CharacterBase character, IAClimbObject climbTarget, Vector3 attachOffset)
    {
        _character = character;
        this.climbTarget = climbTarget;
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

        // ������Ʈ���� ���� ����� ������ ���̱�
        if (climbTarget != null)
        {
            float halfHeight = _character.transform.localScale.y * 0.5f;
            Vector3 origin = _character.transform.position - _character.transform.forward * rayOriginBackOffset + Vector3.up * halfHeight;
            Vector3 direction = _character.transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f, LayerMask.GetMask("Climbable")))
            {
                Vector3 surfacePoint = hit.point + hit.normal * surfaceOffset;
                _character.transform.position = surfacePoint;

                // ������Ʈ ������ �ٶ󺸰�
                Vector3 forwardDir = -hit.normal;
                forwardDir.y = 0;
                _character.transform.forward = forwardDir.normalized;
            }
            else
            {
                // Raycast ���� �� ClosestPoint�� ���� ��ġ ���ϱ�
                Collider col = climbTarget.GetComponent<Collider>();
                Vector3 climbPoint = col.ClosestPoint(_character.transform.position) + _attachOffset;
                _character.transform.position = climbPoint;

                Vector3 dirToCenter = (climbTarget.transform.position - _character.transform.position).normalized;
                dirToCenter.y = 0;
                _character.transform.forward = dirToCenter;
            }
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
