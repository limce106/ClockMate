using UnityEngine;

public class IAChargeStation : MonoBehaviour, IInteractable
{   
    [SerializeField] private IATurret turret;

    public bool CanInteract(CharacterBase character)
    {
        Holder holder = character.GetComponentInChildren<Holder>();
        return character is Hour && holder.IsHolding<IABattery>();
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
        holder.RemoveHoldingObj();
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
            collider.enabled = true;
        }
        Charge();
        return true;
    }

    private void Charge()
    {
        turret.Charge();
    }
}
