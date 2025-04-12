using System;
using System.Collections;
using UnityEngine;
using static Define.Character;

/// <summary>
/// 아워, 밀리 공통 기능 담당 베이스 클래스 ->
/// FSM, 이동, 점프, 상호작용, 바닥감지 모두 처리
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class PlayerBase : MonoBehaviour
{
    [field: SerializeField] public CharacterId CharacterId { get; private set; }
    [field: SerializeField] public PlayerInputHandler Input { get; private set; }
    [field: SerializeField] public CharacterStatsSO Stats { get; private set; }

    public Rigidbody Rb {get; private set;}
    public bool IsGrounded {get; private set;}
    public int JumpCount {get; private set;}

    private StateMachine _stateMachine;
    public string CurrentStateName => _stateMachine?.CurrentState?.GetType().Name ?? "None";
    public IState IdleState {get; private set;}
    public IState JumpState {get; private set;}

    // AbilityExecutor 연결용
    public Action OnInteract = delegate { };
    
    // 바닥 감지 Raycast 관련 설정 
    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] private LayerMask groundLayer;

    protected virtual void Init()
    {
        Rb = GetComponent<Rigidbody>();
        
        IsGrounded = true;
        JumpCount = 0;
        
        _stateMachine = new StateMachine();
        IdleState = new IdleState(this);
        JumpState = new JumpState(this);
        
        _stateMachine.ChangeState(IdleState);
    }

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Update()
    {
        UpdateGroundedStatus();
        _stateMachine.Update();
    }

    /// <summary>
    /// 바닥감지 (Raycast 기반)
    /// </summary>
    private void UpdateGroundedStatus()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            if (!IsGrounded) JumpCount = 0;
            IsGrounded = true;

            Debug.DrawRay(origin, Vector3.down * groundCheckDistance, Color.green);
        }
        else
        {
            IsGrounded = false;
            Debug.DrawRay(origin, Vector3.down * groundCheckDistance, Color.red);
        }
    }
    
    /// <summary>
    /// 이동 처리 (xz 축 이동, y 속도 유지)
    /// </summary>
    public void Move(Vector3 direction)
    {
        Vector3 move = direction * Stats.walkSpeed;

        // 공중에서 방향 전환 가능하도록 현재 y 유지 + xz 보간도 고려 가능
        Rb.velocity = new Vector3(move.x, Rb.velocity.y, move.z);
    }

    /// <summary>
    /// 점프 처리 (1단/이단 점프)
    /// </summary>
    public bool TryJump()
    {
        if (IsGrounded)
        {
            PerformJump(Stats.jumpPower);
            JumpCount = 1;
            return true;
        }
        else if (Stats.canDoubleJump && JumpCount < 2)
        {
            PerformJump(Stats.doubleJumpPower);
            JumpCount++;
            return true;
        }

        return false;
    }

    private void PerformJump(float power)
    {
        transform.position += Vector3.up * 0.05f; // collider 겹침 방지, 살짝 띄우기
        Rb.velocity = new Vector3(Rb.velocity.x, 0f, Rb.velocity.z); // 점프 전 y 속도 제거
        Rb.AddForce(Vector3.up * power, ForceMode.Impulse);
        IsGrounded = false;
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void ChangeState(IState newState)
    {
        _stateMachine.ChangeState(newState);
    }

}
