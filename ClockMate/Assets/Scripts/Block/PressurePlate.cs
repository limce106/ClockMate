using System;
using System.Collections;
using System.Collections.Generic;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class PressurePlate : ResettableBase, IPunObservable
{
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Color _initialColor;

    private MeshRenderer _meshRenderer;
    private Material _materialInstance;
    
    private Coroutine _pressCoroutine;
    
    [Header("Pressure Plate Properties")]
    [SerializeField] private float pressOffsetY = 0.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Color pressedColor = Color.green;
    [SerializeField] private CharacterName character;
  
    private Vector3 _endPoint;
    private bool _isPressed;
    private bool _isLocked;
    public bool IsFullyPressed { get; private set; }
    
    private Vector3 _lastPlatePosition;
    private Transform _attachedTransform;
    
    protected override void Init()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _materialInstance = _meshRenderer.material;
        }
        _endPoint = transform.position + new Vector3(0f, -pressOffsetY, 0f);
        _isPressed = false;
        _isLocked = false;
        IsFullyPressed = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidCharacter(other) || !IsValidDirection(other)) return;

        Debug.Log("위에서 발판 밟음");
        // 캐릭터가 발판 따라가게
        _attachedTransform = other.GetComponentInParent<CharacterBase>().transform;
        _lastPlatePosition = transform.position;
        
        SetPressed(true); // 발판 내려가게
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidCharacter(other) || _isLocked) return;

        if (_attachedTransform != null && other.transform.root == _attachedTransform)
        {
            _attachedTransform = null;
        }
        
        Debug.Log("발판에서 내려옴");
        if (IsFullyPressed)
        {
            IsFullyPressed = false;
            _materialInstance.color = _initialColor;
        }
        SetPressed(false); // 발판 올라가게
    }
    private void FixedUpdate()
    {
        if (_attachedTransform is not null)
        {
            Vector3 delta = transform.position - _lastPlatePosition;
            _attachedTransform.position += delta;
        }

        _lastPlatePosition = transform.position;
    }
    private bool IsValidCharacter(Collider other)
    {
        var characterComponent = other.GetComponentInParent<CharacterBase>();
        if (characterComponent == null)
        {
            Debug.Log("캐릭터 컴포넌트 없음");
            return false;
        }

        return character.GetCharacterType().IsInstanceOfType(characterComponent);
    }
    private bool IsValidDirection(Collider other)
    {
        float yDifference = other.bounds.center.y - transform.position.y;
        if (yDifference < 0.2f) // 상황에 따라 조정
        {
            Debug.Log("위에서 밟은 것이 아님");
            return false;
        }

        return true;
    }

    public void LockState()
    {
        _isLocked = true;
    }

    private void SetPressed(bool state)
    {
        if (_isPressed == state) return;

        _isPressed = state;

        if (_pressCoroutine != null)
        {
            StopCoroutine(_pressCoroutine);
        }
        _pressCoroutine = StartCoroutine(PressRoutine(_isPressed));
    }

    private IEnumerator PressRoutine(bool pressed)
    {
        Vector3 target = pressed ? _endPoint : _initialPosition;
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        // 정확히 도달한 후 상태 및 색상 반영
        transform.position = target;

        if (pressed)
        {
            IsFullyPressed = true;
            _materialInstance.color = pressedColor;
        }
    }

    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialColor = _materialInstance.color;
    }

    [PunRPC]
    public override void ResetObject()
    {
        if (this == null) return;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        _materialInstance.color = _initialColor;
        
        if (_pressCoroutine != null)
        {
            StopCoroutine(_pressCoroutine);
            _pressCoroutine = null;
        }
        _isPressed = false;
        _isLocked = false;
        IsFullyPressed = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);

            Color color = _materialInstance.color;
            stream.SendNext(color.r);
            stream.SendNext(color.g);
            stream.SendNext(color.b);
            stream.SendNext(color.a);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();

            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext();
            float a = (float)stream.ReceiveNext();
            _materialInstance.color = new Color(r, g, b, a);
        }
    }
}
