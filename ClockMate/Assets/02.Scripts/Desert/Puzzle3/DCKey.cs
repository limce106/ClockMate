using DefineExtension;
using Photon.Pun;
using UnityEngine;

public class DCKey : MonoBehaviourPun, IDoorCondition, IInteractable
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
        _holder.RemoveHoldingObj(true);
        NetworkExtension.RunNetworkOrLocal(
            LocalUpdateCondition,
            () => photonView.RPC(nameof(RPC_UpdateDoorConditionWithKey), RpcTarget.All));
        return true;
    }
    
    private void LocalUpdateCondition()
    {
        useKey = true;
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
    }
    
    [PunRPC]
    public void RPC_UpdateDoorConditionWithKey()
    {
        LocalUpdateCondition();
    }
    
}
