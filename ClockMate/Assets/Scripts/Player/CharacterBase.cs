using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private int _jumpCount;
    private int _maxJumpCount;

    private Dictionary<Type, IState> _states;

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
        _maxJumpCount = Stats.canDoubleJump ? 2 : 1;

        _states = new Dictionary<Type, IState>();
        _states.Add(typeof(IdleState), new IdleState(this));
        _stateMachine = new StateMachine(_states[typeof(IdleState)]);

        _groundChecker = new GroundChecker(GetComponent<Collider>(), groundCheckDistance, groundLayer);
    }


    /// <summary>
    /// 최대 점프 횟수 이상이면 false 반환
    /// </summary>
    public bool CanJump()
    {
        return JumpCount < _maxJumpCount;
    }
    
    /// <summary>
    /// 실제 점프 수행 로직
    /// </summary>
    public void PerformJump()
    {
        float jumpPower = (JumpCount == 0 ? Stats.jumpPower : Stats.doubleJumpPower) + 5f;
        _rb.MovePosition(transform.position + Vector3.up * 0.05f); // collider 겹침 방지, 살짝 띄우기
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); // 점프 전 y 속도(추락 속도) 제거
        _rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); // 점프 힘 적용
        JumpCount++;
    }

    public void ResetJumpCount()
    {
        if(JumpCount > 0)
        {
            JumpCount = 0;

        }
    }

    /// <summary>
    /// 상하좌우 이동 로직 (xz 축 이동, y 속도 유지)
    /// </summary>
    public void Move(Vector3 direction)
    {
        bool isGrounded = IsGrounded; // 캐싱

        // 공중 제어력 감소 & 속도 변경 제한
        float controlFactor = isGrounded ? 1f : 0.8f;
        float maxVelocityChange = isGrounded ? 10f : 4f;

        // 현재 속도 및 목표 속도 계산
        Vector3 currentVelocity = _rb.velocity;
        Vector3 targetVelocity = direction.normalized * (Stats.walkSpeed * controlFactor);

        // y축 제외한 속도 차이 계산 후 제한
        Vector3 desiredChange = targetVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 clampedChange = Vector3.ClampMagnitude(desiredChange, maxVelocityChange);

        // 물리 엔진을 통해 부드럽게 가속도 적용 (y축은 유지)
        _rb.AddForce(new Vector3(clampedChange.x, 0f, clampedChange.z), ForceMode.VelocityChange);
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void ChangeState<T>() where T : IState
    {
        var type = typeof(T);

        if (!_states.TryGetValue(type, out var state))
        {
            // 생성자 중 CharacterBase 하나 받는 걸 찾음
            var ctor = type.GetConstructor(new[] { typeof(CharacterBase) });
            if (ctor == null)
                throw new Exception($"{type} 클래스에 맞는 생성자가 없음");

            state = (IState)ctor.Invoke(new object[] { this });
            _states[type] = state;
        }

        _stateMachine.ChangeStateTo(state);
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
