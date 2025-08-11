using DefineExtension;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class IAClockHand : MonoBehaviour, IInteractable
{
    [SerializeField] private CharacterName ControllerName;      // �� �� �ִ� ĳ����
    private float rotationSpeed = 30f;
    private int fixedRotationDirection = 0;  // 1: �ð� ����, -1: �ݽð� ����

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    private Rigidbody _rb;
    private CharacterBase _controller;
    private bool _isControlled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "������";
    }


    void Update()
    {
        if (!_isControlled) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitControl();
            return;
        }

        if (Input.GetKey(KeyCode.W) && fixedRotationDirection != 0)
        {
            transform.Rotate(0f, fixedRotationDirection * rotationSpeed * Time.deltaTime, 0f);
        }
    }

    public bool CanInteract(CharacterBase character)
    {
        if (_controller == null)
            return false;

        if (_controller.Name != ControllerName)
            return false;

        return true;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        _isControlled = true;
        _controller.ChangeState<PushState>(transform);
        _controller.InputHandler.enabled = false;
        _rb.isKinematic = false;

        _controller.GetComponent<Rigidbody>().isKinematic = true;
        _controller.transform.SetParent(transform, true);

        fixedRotationDirection = GetDirectionFromView();

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        // �÷��̾� �ٶ󺸴� ���� ����
        Vector3 direction = transform.position - character.transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        character.transform.rotation = targetRotation;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        return true;
    }

    /// <summary>
    /// �÷��̾� �ü� ������� �ð�/�ݽð� ȸ�� ���� ����
    /// </summary>
    /// <returns></returns>
    private int GetDirectionFromView()
    {
        Vector3 forward = _controller.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 centerToHand = transform.position;
        centerToHand.y = 0;
        centerToHand.Normalize();

        float cross = Vector3.Cross(centerToHand, forward).y;

        if (cross > 0.1f) return 1;
        else if (cross < -0.1f) return -1;
        else return 0;
    }

    private void ExitControl()
    {
        _isControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;
        _rb.isKinematic = true;

        _controller.transform.SetParent(null, true);
        _controller.GetComponent<Rigidbody>().isKinematic = false;
        _controller = null;

        fixedRotationDirection = 0;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;

        // collider �ٽ� Ȱ��ȭ
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.IsPlayerCollider())
        {
            _controller = collision.gameObject.GetComponent<CharacterBase>();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.IsPlayerCollider())
        {
            _controller = null;
        }
    }
}
