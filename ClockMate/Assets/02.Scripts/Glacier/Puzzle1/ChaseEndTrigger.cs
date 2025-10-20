using UnityEngine;

public class ChaseEndTrigger : MonoBehaviour
{
    [SerializeField] ChaseControlModule controlModule;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sled"))
        {
            controlModule.FinishChase();
        }
    }
}
