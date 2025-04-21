using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

/// <summary>
/// 아워, 밀리 공통 동작 처리: 
/// FSM, 이동, 점프, 상호작용, 바닥감지 모두 처리
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterBase : MonoBehaviour
{
    [field: SerializeField] public CharacterStatsSO OriginalStats { get; private set; }
    
    public CharacterStatsSO Stats { get; private set; }

    public bool IsGrounded => _groundChecker.IsGrounded();
    
    private int JumpCount
    {
        get => IsGrounded ? 0 : _jumpCount;
        set => _jumpCount = value;
    }

    private StateMachine _stateMachine;
    private Rigidbody _rb;
    private GroundChecker _groundChecker;
    private Vector3 _moveDirection;
    private int _jumpCount;
    private int _maxJumpCount;

    public IState IdleState {get; private set;}
    public IState JumpState {get; private set;}
    public IState MoveState {get; private set;}

    // 바닥 감지 관련 설정 
    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 0.1f; // 감지 거리
    [SerializeField] private LayerMask groundLayer = ~0; // 기본값: 모든 오브젝트

    protected List<IAbility> abilities = new();

    protected virtual void Awake()
    {
        Init();
    }

    protected void Update()
    {
        _stateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        _stateMachine?.FixedUpdate();
    }

    protected virtual void Init()
    {
        Stats = Instantiate(OriginalStats);
        _rb = GetComponent<Rigidbody>();
        _stateMachine = new StateMachine();
        _maxJumpCount = Stats.canDoubleJump ? 2 : 1;

        IdleState = new IdleState(this);
        JumpState = new JumpState(this);
        MoveState = new WalkState(this);
        _stateMachine.ChangeStateTo(IdleState);

        _groundChecker = new GroundChecker(GetComponent<Collider>(), groundCheckDistance, groundLayer);
    }


    /// <summary>
    /// 점프 처리 (1단/2단 점프)
    /// </summary>
    public bool TryJump()
    {
        // 최대 점프 횟수 이상이면 실패
        if (JumpCount >= _maxJumpCount) return false;
        
        float jumpPower = JumpCount == 0 ? Stats.jumpPower : Stats.doubleJumpPower;
        PerformJump(jumpPower);
        JumpCount++;
        return true;
    }
    
    /// <summary>
    /// 실제 점프 수행 로직
    /// </summary>
    private void PerformJump(float power)
    {
        _rb.MovePosition(transform.position + Vector3.up * 0.05f); // collider 겹침 방지, 살짝 띄우기
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); // 점프 전 y 속도(추락 속도) 제거
        _rb.AddForce(Vector3.up * power, ForceMode.Impulse); // 점프 힘 적용
    }

    /// <summary>
    /// 상하좌우 이동 로직 (xz 축 이동, y 속도 유지)
    /// </summary>
    public void Move()
    {
        bool isGrounded = IsGrounded; // 캐싱

        // 공중 제어력 감소 & 속도 변경 제한
        float controlFactor = isGrounded ? 1f : 0.7f;
        float maxVelocityChange = isGrounded ? 10f : 4f;

        // 현재 속도 및 목표 속도 계산
        Vector3 currentVelocity = _rb.velocity;
        Vector3 targetVelocity = _moveDirection.normalized * (Stats.walkSpeed * controlFactor);

        // y축 제외한 속도 차이 계산 후 제한
        Vector3 desiredChange = targetVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 clampedChange = Vector3.ClampMagnitude(desiredChange, maxVelocityChange);

        // 물리 엔진을 통해 부드럽게 가속도 적용 (y축은 유지)
        _rb.AddForce(new Vector3(clampedChange.x, 0f, clampedChange.z), ForceMode.VelocityChange);
    }

    /// <summary>
    /// 움직이는 방향 설정, PlayerInputHandler에서 이동키 입력에 따라 설정함
    /// </summary>
    public void UpdateMoveDirection(Vector3 direction)
    {
        _moveDirection = direction;
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void TryChangeState(IState newState)
    {
        _stateMachine.ChangeStateTo(newState);
    }
    
    // Stats를 외부에서 교체, 디버그용
    public void OverrideStats(CharacterStatsSO newStats)
    {
        Stats.jumpPower = newStats.jumpPower;
        Stats.doubleJumpPower = newStats.doubleJumpPower;
        Stats.walkSpeed = newStats.walkSpeed;
    }

    public void TryInteract()
    {
        // 상호작용 시도 로직
    }

    /// <summary>
    ///  능력 추가
    /// </summary>
    public void AddAbility(IAbility ability)
    {
        if (ability != null && !abilities.Contains(ability))
        {
            abilities.Add(ability);
        }
    }

    public void RestAbilities()
    {
        abilities.Clear();
    }

    public void TryUseAbility()
    {
        foreach (IAbility ability in abilities)
        {
            if (ability.Use()) break; // 능력 사용 성공하면 반복문 끝내기
        }
    }

}
