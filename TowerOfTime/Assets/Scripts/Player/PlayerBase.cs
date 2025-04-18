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
    [field: SerializeField] public CharacterStatsSO OriginalStats { get; private set; }
    public CharacterStatsSO Stats { get; private set; }

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
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckOffset = 0.6f;
    [SerializeField] private LayerMask groundLayer = ~0; // 기본값: 모든 오브젝트
    private GroundChecker _groundChecker;

    protected virtual void Init()
    {
        Stats = Instantiate(OriginalStats);
        Rb = GetComponent<Rigidbody>();
        
        IsGrounded = true;
        JumpCount = 0;
        
        _stateMachine = new StateMachine();
        IdleState = new IdleState(this);
        JumpState = new JumpState(this);
        
        _stateMachine.ChangeState(IdleState);
        _groundChecker = new GroundChecker(transform, GetHalfWidth(), groundCheckDistance, groundCheckOffset, groundLayer);
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
        if (_groundChecker.IsGrounded())
        {
            if (!IsGrounded)
            {
                JumpCount = 0;
            }

            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
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

        if (Stats.canDoubleJump && JumpCount < 2)
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
    }

    /// <summary>
    /// 플레이어 몸통 두께 기반으로 Ray Offset 자동 설정
    /// </summary>
    private float GetHalfWidth()
    {
        Collider col = GetComponent<Collider>();

        if (col is CapsuleCollider capsule)
        {
            return capsule.radius * 0.95f;
        }

        Debug.LogWarning("플레이어 콜라이더 찾지 못함. Default halfWidth 사용");
        return 0.25f; // 콜라이더 다를 시 기본값
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
    /// 상태 전환
    /// </summary>
    public void ChangeState(IState newState)
    {
        _stateMachine.ChangeState(newState);
    }

    public virtual void EnableControl(bool enable)
    {
        if (Input != null)
        {
            Input.enabled = enable;
        }
    }
    
    // Stats를 외부에서 교체
    public void OverrideStats(CharacterStatsSO newStats)
    {
        Stats.jumpPower = newStats.jumpPower;
        Stats.doubleJumpPower = newStats.doubleJumpPower;
        Stats.walkSpeed = newStats.walkSpeed;
        Stats.climbSpeed = newStats.climbSpeed;
    }

}
