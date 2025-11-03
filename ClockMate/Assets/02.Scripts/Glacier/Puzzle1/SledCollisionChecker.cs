using System;
using UnityEngine;

public class SledCollisionChecker : MonoBehaviour
{
    [SerializeField] private SledController sledController;

    private void Start()
    {
        if (sledController == null)
        {
            sledController = GetComponentInParent<SledController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            sledController.Drown();
        } 
        else if (other.CompareTag("Ground") && sledController.photonView.IsMine)
        {
            sledController.IsGrounded = true;
            Debug.Log($"[Sled Collision Checker] Trigger Enter {other.name}]");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!sledController.photonView.IsMine) return;
        if (other.CompareTag("Ground") && !sledController.IsGrounded)
        {
            sledController.IsGrounded = true;
            Debug.Log($"[Sled Collision Checker] Trigger Stay {other.name}]");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!sledController.photonView.IsMine) return;

        if (!other.CompareTag("Ground")) return;
        
        sledController.IsGrounded = false;
        Debug.Log($"[Sled Collision Checker] Trigger Exit {other.name}]");

    }
}
