using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define.Character;

public class ClimbState : IState
{
    private readonly CharacterBase _character;
    public readonly ClimbObjectBase climbTarget;

    private float _climbSpeed = 3f;
    private const float Margin = 0.3f;

    private Rigidbody _rb;
    private bool playerAttached = false;
    private SoundHandle _climbSfxHandle = default;

    public bool isPlayingClimbEnd { private set; get; } = false; // 정상 도달 애니메이션 실행 중 여부
    public const float climbEndDuration = 4f;

    public ClimbState(CharacterBase character, ClimbObjectBase climbTarget)
    {
        _character = character;
        this.climbTarget = climbTarget;
    }

    public void Enter()
    {
        _rb = _character.GetComponent<Rigidbody>();
        StartClimbing();
    }

    public void FixedUpdate()
    {
        if (!playerAttached || isPlayingClimbEnd)
            return;

        if (_character.photonView.IsMine)
        {
            float characterY = _character.transform.position.y;

            // 정상 도달
            if (characterY >= climbTarget.topY + Margin && _character.InputHandler.climbingState == ClimbingState.Up)
            {
                if (!isPlayingClimbEnd)
                {
                    _character.photonView.RPC("RPC_ClimbEnd", RpcTarget.All, climbTarget.TopTargetPoint.position);
                }
                return;
            }
            // 하단 도달
            else if (characterY <= climbTarget.bottomY - Margin)
            {
                _character.photonView.RPC("RPC_StopClimbing", RpcTarget.All);
                return;
            }
        }
    }

    public void Update() { }

    public void Exit() { }

    void StartClimbing()
    {
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        climbTarget.AttachTo(_character);
        playerAttached = true;
    }

    /// <summary>
    /// 기어오르기/내려가기
    /// </summary>
    public void Climb(float vertical)
    {
        if (isPlayingClimbEnd)
        {
            StopClimbSfx();

            _rb.velocity = Vector3.zero;
            return;
        }

        _rb.velocity = new Vector3(0f, vertical * _climbSpeed, 0f);
        StartClimbSfx();
    }

    /// <summary>
    /// 기어오르기 중단
    /// </summary>
    public void StopClimbing()
    {
        StopClimbSfx();

        _character.Anim.SetClimbUp(false);
        _character.Anim.SetClimbDown(false);

        _character.Anim.photonView.RPC("RPC_SetAnimPlayback", RpcTarget.All, true);

        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _character.ChangeState<IdleState>();

        if (_character.photonView.IsMine)
        {
            climbTarget.CloseUI();
            climbTarget.EnableColliders(true);
        }
    }

    /// <summary>
    /// 절벽 정상 위로 이동
    /// </summary>
    public IEnumerator MoveToTop(Vector3 start, Vector3 end, float duration)
    {
        float timer = 0f;

        isPlayingClimbEnd = true;
        StopClimbSfx();

        if (_character.photonView.IsMine)
        {
            _character.InputHandler.enabled = false;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            _character.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        _character.transform.position = end;
        isPlayingClimbEnd = false;

        StopClimbing();

        if (_character.photonView.IsMine)
        {
            _character.InputHandler.enabled = true;
        }
    }

    public void StartClimbSfx()
    {
        if(!_climbSfxHandle.IsValid)
        {
            _climbSfxHandle = SoundManager.Instance.PlaySfx(
                key: "character_climb",
                loop: true,
                pos: _character.transform.position,
                sync: false,
                volume: 1f);
        }
    }

    public void StopClimbSfx()
    {
        if(_climbSfxHandle.IsValid)
        {
            SoundManager.Instance.Stop(_climbSfxHandle);
            _climbSfxHandle = default;
        }
    }
}
