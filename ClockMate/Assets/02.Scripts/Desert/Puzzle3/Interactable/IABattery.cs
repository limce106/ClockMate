using System;
using UnityEngine;

public class IABattery : ResettableBase, IInteractable
{
    private bool _isHeld;

    private Holder _currentHolder;
    
    private UIManager _uiManager;
    private UINotice _uiNotice;

    private Sprite _dropSprite;
    private string _dropString;

    private Transform _initialParent;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public event Action<IABattery> OnUse;

    private void Update()
    {
        if (!_isHeld) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryDrop();
            // 배터리 내려놓기 UI 닫기
            _uiManager.Close(_uiNotice);
        }
    }

    protected override void Init()
    {
        _isHeld = false;
        _uiManager = UIManager.Instance;
        _dropSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _dropString = "내려놓기";
    }
    
    public bool CanInteract(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        return !holder.IsHolding() && character is Hour;
    }

    public void OnInteractAvailable()
    {

    }

    public void OnInteractUnavailable()
    {
        
    }

    public bool Interact(CharacterBase character)
    {
        TryPickUp(character);
            
        // 배터리 내려놓기 UI 표시
        _uiNotice = _uiManager.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_dropSprite);
        _uiNotice.SetText(_dropString);
        
        return true;
    }

    private void TryPickUp(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        if (holder == null) return;
        
        holder.SetHoldingObj(gameObject);

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }
        
        // 필요시 이동 속도 감소 처리 추가
        
        _currentHolder = holder;
        _isHeld = true;
    }

    private void TryDrop()
    {
        _currentHolder?.DropHoldingObj();

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }
        
        // 이동 속도 복구 등 추가 처리
        
        _currentHolder = null;
        _isHeld = false;
    }

    public void UseToCharge()
    {
        _uiManager.Close(_uiNotice);
        ResetObject();
        gameObject.SetActive(false);
        OnUse?.Invoke(this);
    }

    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialParent = gameObject.transform.parent;
    }

    public override void ResetObject()
    {
        _isHeld = false;
        _currentHolder = null;
        gameObject.SetActive(true);
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        ReturnToInitialTransform();
    }

    private void ReturnToInitialTransform()
    {
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        gameObject.transform.SetParent(_initialParent);
    }
}
