using UnityEngine;

public class Holder : MonoBehaviour
{
    private GameObject _holdingObj;
    private Transform _originalParent;

    public bool IsHolding<T>() where T : IInteractable
    {
        if (_holdingObj is null) return false;
        
        _holdingObj.TryGetComponent(out IInteractable interactable);
        return _holdingObj is not null && interactable.GetType() == typeof(T);
    }
    public bool IsHolding()
    {
        return _holdingObj is not null;
    }

    public void SetHoldingObj(GameObject obj)
    {
        _originalParent = obj.transform.parent;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        _holdingObj = obj;
    }

    public void DropHoldingObj()
    {
        _holdingObj.transform.SetParent(_originalParent != null ? _originalParent : null);
        _holdingObj.transform.position = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
        _holdingObj = null;
        _originalParent = null;
    }

    public void RemoveHoldingObj()
    {
        Destroy(_holdingObj);
        _holdingObj = null;
        _originalParent = null;
    }
}
