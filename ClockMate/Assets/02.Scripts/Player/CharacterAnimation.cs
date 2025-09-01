using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class CharacterAnimation : MonoBehaviourPun
{
    [Header("Refs")]
    [SerializeField] private Animator animator;         // 없으면 자동 검색
    [SerializeField] private CharacterBase character;   // IsGrounded 참조

    [Header("Animator Params")]
    [SerializeField] private string pSpeed = "Speed";
    [SerializeField] private string pIsGrounded = "IsGrounded";
    [SerializeField] private string pJump = "Jump";
    [SerializeField] private string pFanFly = "FanFly";
    [SerializeField] private string pIsJumping = "IsJumping";

    [Header("Speed Smoothing")]
    [SerializeField] private float speedLerp = 0.2f;    // 애니용 속도 평활화

    [Header("Walk")]
    [SerializeField] private string walkStateName = "Walk";
    [SerializeField] private float[] walkMarks = { 0.23f, 0.73f }; // 루프 내 접지 지점(0~1)
    [SerializeField] private float minSpeedForStep = 0.2f;         // 너무 느리면 미재생

    // hashes
    private int _hSpeed, _hIsGrounded, _hJump, _hFanFly;

    private Vector3 _prevPos;
    private float _lastPhase; // 0~1
    
    private bool _wasGroundedForNetwork;
    private CharacterSfx _characterSfx;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (!animator)  animator  = GetComponentInChildren<Animator>(true);
        if (!character) character = GetComponent<CharacterBase>();

        _hSpeed = Animator.StringToHash(pSpeed);
        _hIsGrounded = Animator.StringToHash(pIsGrounded);
        _hJump = Animator.StringToHash(pJump);
        _hFanFly = Animator.StringToHash(pFanFly);

        if (animator)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }
        _prevPos = transform.position;
        if (character)
        {
            _wasGroundedForNetwork = character.IsGrounded;
        }
        _characterSfx = GetComponent<CharacterSfx>();
    }

    private void OnEnable()
    {
        _prevPos = transform.position;
        _lastPhase = 0f;
    }

    private void Update()
    {
        if (!animator || !character) return;

        // transform 델타로 속도 계산
        Vector3 curr = transform.position;
        Vector3 positionDelta = curr - _prevPos;
        float deltaTime = Mathf.Max(Time.deltaTime, 1e-6f);
        float planarSpeed = new Vector2(positionDelta.x, positionDelta.z).magnitude / deltaTime;
        _prevPos = curr;

        // 애니 파라미터 갱신
        float prev = animator.GetFloat(_hSpeed);
        float smoothedPlanarSpeed = Mathf.Lerp(prev, planarSpeed, speedLerp);
        animator.SetFloat(_hSpeed, smoothedPlanarSpeed);
        
        if (photonView.IsMine)
        {
            // 로컬 플레이어는 직접 상태를 계산, 변경됐을 때만 RPC 호출
            bool isGroundedNow = character.IsGrounded;
            if (isGroundedNow != _wasGroundedForNetwork)
            {
                _wasGroundedForNetwork = isGroundedNow;
                photonView.RPC(nameof(RPC_SyncIsGrounded), RpcTarget.All, isGroundedNow);
            }
        }

        UpdateFootstepPhase(smoothedPlanarSpeed);
    }

    /// <summary>
    /// 속도에 따라 발소리 sfx를 처리한다
    /// </summary>
    private void UpdateFootstepPhase(float currSpeed)
    {
        if (animator == null || character == null) return;

        // 지면 미접촉이면 발소리 없음(phase만 업데이트)
        AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
        float phaseNow = st.normalizedTime % 1f;

        if (!character.IsGrounded)
        {
            _lastPhase = phaseNow;
            return;
        }

        // Walk 상태에서만 처리
        if (!st.IsName(walkStateName))
        {
            _lastPhase = phaseNow;
            return;
        }

        if (currSpeed >= minSpeedForStep)
        {
            bool wrapped = phaseNow < _lastPhase; // 1→0 랩 감지
            for (int i = 0; i < walkMarks.Length; i++)
            {
                float m = walkMarks[i];
                if ((!wrapped && _lastPhase < m && phaseNow >= m) || ( wrapped && (_lastPhase < m || phaseNow >= m)))
                {
                    _characterSfx?.PlayFootstepSound();
                }
            }
        }

        _lastPhase = phaseNow;
    }

    /// <summary>
    /// 동기화된 점프 애니메이션을 재생한다. 소유자가 호출하여야 한다. 
    /// </summary>
    public void PlayJump()
    {
        if (animator && photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_PlayJump), RpcTarget.All);
        }
    }


    [PunRPC] 
    private void RPC_PlayJump()
    {
        animator.SetTrigger(_hJump);
        _characterSfx?.PlayJumpSound();
        ResetDelta();
    }
    
    [PunRPC]
    private void RPC_SyncIsGrounded(bool isGrounded)
    {
        animator.SetBool(_hIsGrounded, isGrounded);
    }

    public void SetFanFly(bool on)
    {
        if (animator && photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetFanFly), RpcTarget.All, on);
        }
    }

    [PunRPC] 
    private void RPC_SetFanFly(bool on)
    {
        animator.SetBool(_hFanFly, on);
    }

    public void ResetDelta() => _prevPos = transform.position;
}