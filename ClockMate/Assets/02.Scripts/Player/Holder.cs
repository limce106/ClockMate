using System.Collections;
using DefineExtension;
using Photon.Pun;
using UnityEngine;

public class Holder : MonoBehaviourPun
{
    private GameObject _holdingObj;
    private Transform _originalParent;
    private CharacterBase _character;
    
    private void Awake()
    {
        _character = GetComponentInParent<CharacterBase>();
    }

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
        _character.Anim.PlayPickUp();
        _character.InputHandler.enabled = false; // 줍기 애니메이션 재생동안은 움직임 중단
        StartCoroutine(nameof(PickUpThenSetPos), obj);
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
        _character.Anim.SetCarry(true);
    }
    
    private IEnumerator PickUpThenSetPos(MonoBehaviourPun obj)
    {
        yield return new WaitForSeconds(1.0f);
        // 들어올리는 애니메이션 재생 기다린 뒤 물건 위치 이동, 움직임 재활성화
        _character.InputHandler.enabled = true;
        NetworkExtension.RunNetworkOrLocal(
            () => LocalSetHoldingObj(obj.gameObject),
            () => photonView.RPC(nameof(RPC_SetHoldingObj), RpcTarget.All, obj.photonView.ViewID)
        );
    } 

    [PunRPC]
    public void RPC_SetHoldingObj(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;
        
        LocalSetHoldingObj(view.gameObject);
    }

    public bool TryDropHoldingObj()
    {
        if(_holdingObj == null) return false;
        NetworkExtension.RunNetworkOrLocal(
            LocalDropHoldingObj,
            () => photonView.RPC(nameof(RPC_DropHoldingObj), RpcTarget.All)
        );
        return true;
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

    public void RemoveHoldingObj(bool destroy = false)
    {
        NetworkExtension.RunNetworkOrLocal(
            () => LocalRemoveHoldingObj(destroy),
            () => photonView.RPC(nameof(RPC_RemoveHoldingObj), RpcTarget.All, destroy)
        );
    }
    
    private void LocalRemoveHoldingObj(bool destroy)
    {
        if (destroy)
        {
            Destroy(_holdingObj);
        }
        _holdingObj = null;
        _originalParent = null;
        _character.Anim.SetCarry(false);
    }
    
    [PunRPC]
    public void RPC_RemoveHoldingObj(bool destroy)
    {
        LocalRemoveHoldingObj(destroy);
    }
}
