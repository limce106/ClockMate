using Define;
using UnityEngine;

public class IASledTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private SledChaseOrchestrator orchestrator;
    private bool _available = true;

    public bool CanInteract(CharacterBase character)
    {
        return _available;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        _available = false;
        
        if (orchestrator != null)
        {
            orchestrator.RequestStartFromTrigger();
            return true;
        }

        Debug.LogError("[IASledTrigger] Orchestrator not found.");
        return false;
    }
}
