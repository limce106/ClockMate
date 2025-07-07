using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class IAChargeStation : MonoBehaviourPun, IInteractable
{   
    [SerializeField] private IATurret turret;

    public bool CanInteract(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        return character.Name == CharacterName.Hour && holder.IsHolding<IABattery>();
    }

    public void OnInteractAvailable()
    {
        
    }

    public void OnInteractUnavailable()
    {
        
    }

    public bool Interact(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        if(!holder.IsHolding(out IABattery battery)) return false;
        
        battery.UseToCharge();
        holder.RemoveHoldingObj();
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
            col.enabled = true;
        }
        turret.Charge();
        return true;
    }
}
