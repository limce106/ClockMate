using DefineExtension;
using Photon.Pun;
using UnityEngine;

public class Holder : MonoBehaviourPun
{
    private GameObject _holdingObj;
    private Transform _originalParent;

    public bool IsHolding<T>() where T : IInteractable
    {
        if (_holdingObj is null) return false;
        return _holdingObj.TryGetComponent(out T _interactable);
    }
    public bool IsHolding<T>(out T interactable) where T : IInteractable
    {
        if (_holdingObj is null)
        {
            interactable = default(T);
            return false;
        }
        return _holdingObj.TryGetComponent(out interactable);
    }
    public bool IsHolding()
    {
        return _holdingObj is not null;
    }

    public void SetHoldingObj<T>(T obj) where T : MonoBehaviourPun, IInteractable
    {
        NetworkExtension.RunNetworkOrLocal(
            () => LocalSetHoldingObj(obj.gameObject),
            () => photonView.RPC(nameof(RPC_SetHoldingObj), RpcTarget.All, obj.photonView.ViewID)
        );
    }

    private void LocalSetHoldingObj(GameObject obj)
    {
        _originalParent = obj.transform.parent;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (obj.TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
        
        _holdingObj = obj;
    }

    [PunRPC]
    public void RPC_SetHoldingObj(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;
        
        LocalSetHoldingObj(view.gameObject);
    }

    public void DropHoldingObj()
    {
        NetworkExtension.RunNetworkOrLocal(
            LocalDropHoldingObj,
            () => photonView.RPC(nameof(RPC_DropHoldingObj), RpcTarget.All)
        );
    }

    private void LocalDropHoldingObj()
    {
        _holdingObj.transform.SetParent(_originalParent != null ? _originalParent : null);
        _holdingObj.transform.position = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
        
        if (_holdingObj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        if (_holdingObj.TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        
        RemoveHoldingObj();
    }
    
    [PunRPC]
    public void RPC_DropHoldingObj()
    {
        LocalDropHoldingObj();
    }

    public void DestroyHoldingObj()
    {
        Destroy(_holdingObj);
        RemoveHoldingObj();
    }

    public void RemoveHoldingObj()
    {
        _holdingObj = null;
        _originalParent = null;
    }
}
