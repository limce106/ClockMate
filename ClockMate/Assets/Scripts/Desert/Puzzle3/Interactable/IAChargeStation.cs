using UnityEngine;

public class IAChargeStation : MonoBehaviour, IInteractable
{   
    [field: SerializeField] public int ChargeLevel { get; private set; }
    
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
        ChargeLevel++;
    }

    public void UseCharged()
    {
        ChargeLevel--;
    }
}
