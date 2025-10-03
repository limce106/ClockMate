using UnityEngine;

public class Slot : MonoBehaviour
{
    [SerializeField] private IASlotTrigger trigger;
    public bool TriggerActivated { get; private set; }

    public void ActivateTrigger(bool active)
    {
        trigger.gameObject.SetActive(active);
        TriggerActivated = active;
    }

    public void ApplyCogToTrigger(Cog cog)
    {
        trigger.SetCog(cog);   
    }
}
