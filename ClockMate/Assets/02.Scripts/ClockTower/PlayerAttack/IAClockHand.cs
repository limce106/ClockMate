using DefineExtension;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static Define.Character;

public class IAClockHand : MonoBehaviour, IInteractable
{
    [SerializeField] private CharacterName ControllerName;      // 밀 수 있는 캐릭터
    private int _fixedRotationDirection = 0;  // 1: 시계 방향, -1: 반시계 방향

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    [SerializeField] private MeshRenderer meshRenderer;
    private Rigidbody _rb;
    private CharacterBase _controller;
    private bool _isControlled;

    private const float ControllerOffset = 0.7f;
    private const float RotationSpeed = 20f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "나가기";
    }


    void Update()
    {
        if (!_isControlled) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitControl();
            return;
        }

        if (Input.GetKey(KeyCode.W) && _fixedRotationDirection != 0)
        {
            transform.root.Rotate(0f, _fixedRotationDirection * RotationSpeed * Time.deltaTime, 0f);
        }
    }

    public bool CanInteract(CharacterBase character)
    {
        if (character.Name != ControllerName)
            return false;

        Vector3 boundsSize = meshRenderer.bounds.size;
        Vector3 localBoundSize = transform.InverseTransformVector(boundsSize);

        float sideWidth = Mathf.Abs(localBoundSize.x) / 2;
        Vector3 characterLocalPos = transform.InverseTransformPoint(character.transform.position);

        // 캐릭터가 바늘의 앞뒤에 있으면 상호작용 불가
        if (Mathf.Abs(characterLocalPos.x) < sideWidth)
            return false;

        return true;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        _isControlled = true;
        _controller = character;

        if (GetDirectionFromView() == 0)
            return false;

        _fixedRotationDirection = GetDirectionFromView();

        _controller.ChangeState<PushState>(transform);
        _controller.InputHandler.enabled = false;
        _rb.isKinematic = false;

        _controller.GetComponent<Rigidbody>().isKinematic = true;
        _controller.transform.SetParent(transform);

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        SetControllerPos();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        return true;
    }

    /// <summary>
    /// 플레이어 시선 기반으로 시계/반시계 회전 방향 결정
    /// </summary>
    /// <returns></returns>
    private int GetDirectionFromView()
    {
        Vector3 hingePos = GetComponent<HingeJoint>().transform.position;
        Vector3 playerDir = _controller.transform.position - hingePos;

        Vector3 forward = transform.forward; // 바늘 앞 방향
        forward.y = 0;
        forward.Normalize();

        playerDir.y = 0;
        playerDir.Normalize();

        float crossY = Vector3.Cross(forward, playerDir).y;

        if (crossY > 0.1f)
            return -1; // 플레이어가 바늘 왼쪽 → 시계 방향
        else if (crossY < -0.1f)
            return 1; // 오른쪽 → 반시계 방향
        else
            return 0;
    }

    private void SetControllerPos()
    {
        float originControllerY = _controller.transform.position.y;

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 toPlayer = _controller.transform.position - transform.position;
        float side = Vector3.Dot(toPlayer, right);

        Vector3 attachDir = side >= 0 ? right : -right;
        Vector3 targetPos = transform.position + attachDir * ControllerOffset;
        targetPos.y = originControllerY;
        _controller.transform.position = targetPos;

        _controller.transform.rotation = Quaternion.LookRotation(-attachDir);
    }

    private void ExitControl()
    {
        // collider 다시 활성화
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        _isControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;

        _controller.transform.SetParent(null);

        RaycastHit hit;
        if (Physics.Raycast(_controller.transform.position + Vector3.up, Vector3.down, out hit, 5f))
        {
            _controller.transform.position = hit.point + Vector3.up * 0.01f; // 지면 바로 위
            _controller.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * _controller.transform.rotation;
        }

        _controller.GetComponent<Rigidbody>().isKinematic = false;
        _controller = null;

        _rb.isKinematic = true;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
    }
}
