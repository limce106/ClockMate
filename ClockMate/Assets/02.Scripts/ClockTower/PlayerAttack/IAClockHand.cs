using DefineExtension;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class IAClockHand : MonoBehaviour, IInteractable
{
    [SerializeField] private CharacterName ControllerName;      // 밀 수 있는 캐릭터
    private float rotationSpeed = 30f;
    private Transform clockCenterTransform;

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    private Rigidbody _rb;
    private CharacterBase _controller;
    private bool _isColliding = false;
    private bool _isControlled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        clockCenterTransform = transform.parent;
        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "나가기";
    }

    void Update()
    {
        if (!_isControlled) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitControl();
        }

        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += _controller.transform.forward;
        if (Input.GetKey(KeyCode.S)) inputDir -= _controller.transform.forward;
        if (Input.GetKey(KeyCode.A)) inputDir -= _controller.transform.right;
        if (Input.GetKey(KeyCode.D)) inputDir += _controller.transform.right;

        if (inputDir.magnitude < 0.1f)
            return;

        inputDir.y = 0;
        inputDir.Normalize();

        Vector3 centerToHand = transform.position - clockCenterTransform.position;
        centerToHand.y = 0;
        centerToHand.Normalize();

        float cross = Vector3.Cross(centerToHand, inputDir).y;

        float direction = 0f;

        if (cross > 0.1f)
            direction = 1f;
        else if (cross < -0.1f)
            direction = -1f;
        else
            direction = 0f;

        transform.Rotate(0f, direction * rotationSpeed * Time.deltaTime, 0f);
    }

    public bool CanInteract(CharacterBase character)
    {
        if (!_isColliding || _controller == null)
            return false;

        if (character.Name != ControllerName)
            return false;

        return true;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        _isControlled = true;
        _controller = character;
        _controller.ChangeState<PushState>(transform);
        //_controller.InputHandler.enabled = false;
        _rb.isKinematic = false;

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }

        return true;
    }

    private void ExitControl()
    {
        _isControlled = false;
        _controller.ChangeState<IdleState>();
        //_controller.InputHandler.enabled = true;
        _controller = null;
        _rb.isKinematic = true;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;

        // collider 다시 활성화
        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.IsPlayerCollider())
        {
            _isColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.IsPlayerCollider())
        {
            _isColliding = false;
        }
    }
}
