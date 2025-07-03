using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCKey : MonoBehaviour, IDoorCondition, IInteractable
{
    [SerializeField] private bool useKey;
    
    private Holder _holder;

    public bool IsConditionMet()
    {
        return useKey;
    }

    public bool CanInteract(CharacterBase character)
    {
        _holder = character.GetComponentInChildren<Holder>();
        return _holder.IsHolding<IAKey>();
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        useKey = true;
        _holder.DestroyHoldingObj();
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }
        return true;
    }
}
