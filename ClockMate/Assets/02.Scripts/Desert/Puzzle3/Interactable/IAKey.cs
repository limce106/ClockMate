using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAKey : MonoBehaviour, IInteractable
{
     private bool _isHeld;

    private UIManager _uiManager;
    private UINotice _uiNotice;

    private string _dropString;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _isHeld = false;
        _uiManager = UIManager.Instance;
        _dropString = "열쇠로 문을 열자";
    }
    
    public bool CanInteract(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        return !holder.IsHolding();
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
        _uiNotice.SetImageActive(false);
        _uiNotice.SetText(_dropString);
        
        return true;
    }

    private void TryPickUp(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        if (holder == null) return;
        
        holder.SetHoldingObj(this.gameObject);

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }
        
        _isHeld = true;
    }

    private void OnDestroy()
    {
        if (_isHeld)
        {
            _uiManager.Close(_uiNotice);
        }
    }
}
