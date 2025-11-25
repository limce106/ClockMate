using System.Collections;
using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class PressurePlate : ResettableBase
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
    public bool isPressed { get; private set; }
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
        isPressed = false;
        _isLocked = false;
        IsFullyPressed = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidCharacter(other) || !IsValidDirection(other)) return;
        var otherCh = other.GetComponentInParent<CharacterBase>();
        if (!otherCh.photonView.IsMine) return;
        // 캐릭터가 발판 따라가게
        _attachedTransform = otherCh.transform;
        _lastPlatePosition = transform.position;
        
        photonView.RPC(nameof(RPC_SetPressed), RpcTarget.All, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidCharacter(other) || _isLocked) return;

        var otherCh = other.GetComponentInParent<CharacterBase>();
        if (!otherCh.photonView.IsMine) return;
        if (_attachedTransform != null && other.transform.root == _attachedTransform)
        {
            _attachedTransform = null;
        }
        photonView.RPC(nameof(RPC_SetPressed), RpcTarget.All, false);
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
        return other.CompareTag(character.ToString());
    }
    private bool IsValidDirection(Collider other)
    {
        float yDifference = other.bounds.center.y - transform.position.y;
        if (yDifference < 0.4f) // 상황에 따라 조정
        {
            Debug.Log("위에서 밟은 것이 아님");
            return false;
        }

        return true;
    }

    public void SetLockState(bool isLocked)
    {
        _isLocked = isLocked;
    }

    private void SetPressed(bool state)
    {
        if (isPressed == state) return;

        isPressed = state;
        PlaySfx(isPressed);

        if (_pressCoroutine != null)
        {
            StopCoroutine(_pressCoroutine);
        }
        _pressCoroutine = StartCoroutine(PressRoutine(isPressed));
    }

    private void PlaySfx(bool press)
    {
        string sfxKey = press ? "button_press_down" : "button_press_up";
        SoundManager.Instance.PlaySfx(key: sfxKey, pos: transform.position, volume: 0.8f);
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
        isPressed = false;
        _isLocked = false;
        IsFullyPressed = false;
    }
    
    [PunRPC]
    public void RPC_SetPressed(bool pressed)
    {
        if (!pressed)
        {
            if (IsFullyPressed)
            {
                IsFullyPressed = false;
                _materialInstance.color = _initialColor;
            }
        }
        SetPressed(pressed);
    }
}
