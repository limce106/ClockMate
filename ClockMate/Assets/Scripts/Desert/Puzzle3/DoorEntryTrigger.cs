using System.Collections;
using System.Collections.Generic;
using DefineExtension;
using UnityEngine;

public class DoorEntryTrigger : MonoBehaviour
{
    [SerializeField] DoorConditionController doorConditionController;
    private void OnTriggerExit(Collider other)
    {
        if (other.IsPlayerCollider())
        {
            doorConditionController.EnterDoor();
        }
    }
}
