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
    public bool isPlayingClimbEnd { private set; get; } = false;
    private const float climbEndDuration = 4f;

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

        float characterY = _character.transform.position.y;
        if (characterY >= climbTarget.topY + Margin && _character.InputHandler.climbingState == ClimbingState.Up)
        {
            _character.GetComponent<Rigidbody>().velocity = Vector3.zero;
            _character.Anim.PlayClimbEnd();
            _character.StartCoroutine(MoveToTop(_character.transform.position, climbTarget.TopTargetPoint.position, climbEndDuration));
            return;
        }
        else if (characterY <= climbTarget.bottomY - Margin)
        {
            StopClimbing();
            return;
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

    public void Climb(float vertical)
    {
        if (isPlayingClimbEnd)
        {
            _rb.velocity = Vector3.zero;
            return;
        }

        _rb.velocity = new Vector3(0f, vertical * _climbSpeed, 0f);
    }

    public void StopClimbing()
    {
        _character.Anim.SetClimbDown(false);
        _character.Anim.SetClimbUp(false);
        _character.Anim.SetAnimPlayback(true);

        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _character.ChangeState<IdleState>();
        climbTarget.CloseUI();

        climbTarget.EnableColliders(true);
    }

    private IEnumerator MoveToTop(Vector3 start, Vector3 end, float duration)
    {
        float timer = 0f;

        isPlayingClimbEnd = true;
        _character.InputHandler.enabled = false;

        while(timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            _character.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        _character.transform.position = end;
        isPlayingClimbEnd = false;
    }
}
