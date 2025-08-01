using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class SledController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxYawAngle; // 좌우 최대 회전 각도
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private bool isMoving;
    
    private Rigidbody _rb;
    private bool _jumpRequested;
    private float _currentYaw;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!isMoving || GameManager.Instance.SelectedCharacter != CharacterName.Hour) return;
        // 움직이는 중이고 아워라면 
        
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            // 땅에 있을 때 스페이스 눌리면 점프 요청 처리
            _jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving || GameManager.Instance.SelectedCharacter != CharacterName.Hour) return;

        MoveForward(); 
        HandleTurn();

        if (_jumpRequested)
        {
            Jump();
            _jumpRequested = false;
        }
    }

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        _jumpRequested = false;
        _currentYaw = 0f;
    }

    /// <summary>
    /// 썰매가 바라보는 정면 방향으로 이동
    /// </summary>
    private void MoveForward()
    {
        _rb.MovePosition(_rb.position + transform.forward * (moveSpeed * Time.fixedDeltaTime));
    }
    
    /// <summary>
    /// 좌/우 회전 처리
    /// </summary>
    private void HandleTurn()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.A)) turn = -1f; // 왼쪽
        if (Input.GetKey(KeyCode.D)) turn = 1f; // 오른쪽

        if (turn == 0f) return;
        // 회전 입력이 존재할 경우 회전 처리
        
        float deltaYaw = turn * rotationSpeed * Time.fixedDeltaTime; // 한 프레임마다 회전할 양
        float nextYaw = Mathf.Clamp(_currentYaw + deltaYaw, -maxYawAngle, maxYawAngle); // 회전 누적 제한
        float clampedDelta = nextYaw - _currentYaw; // 제한 반영된 실제 회전량
        _currentYaw = nextYaw; // _currentYaw 갱신

        // 회전 적용
        Quaternion deltaRotation = Quaternion.Euler(Vector3.up * clampedDelta); // y축 기준
        _rb.MoveRotation(_rb.rotation * deltaRotation);
    }

    
    private void Jump()
    {
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// 썰매가 지면에 닿아있는지 여부를 반환
    /// </summary>
    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
}
