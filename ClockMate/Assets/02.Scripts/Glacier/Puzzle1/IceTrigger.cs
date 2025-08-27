using UnityEngine;

public class IceTrigger : MonoBehaviour
{
    [SerializeField] private SledChaseOrchestrator chaseOrchestrator;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sled")) return;
        GetComponent<Collider>().enabled = false;
        chaseOrchestrator.RequestIceEventFromTrigger();
    }
}
