using UnityEngine;

public class IAIceBallTrigger : MonoBehaviour, IInteractable    
{
    [SerializeField] private IceBall iceBall;
    
    public bool CanInteract(CharacterBase character)
    {
        return character is Hour && !iceBall.IsControlled;
    }

    public void OnInteractAvailable()
    {

    }

    public void OnInteractUnavailable()
    {
        
    }

    public bool Interact(CharacterBase character)
    {
        if (character is not Hour hour) return false;

        // 상호작용 탐지되지 않도록 collider 비활성화
        EnableCollider(false);
        
        iceBall.StartControl(hour);
        iceBall.OnControlEnd -= EnableCollider;
        iceBall.OnControlEnd += EnableCollider;

        return true;
    }

    private void EnableCollider(bool enable)
    {
        if (TryGetComponent(out Collider col))
        {
            col.enabled = enable;
        }
    }
}
