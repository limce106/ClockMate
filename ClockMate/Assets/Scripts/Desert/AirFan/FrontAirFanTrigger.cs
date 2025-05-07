using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontAirFanTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Milli")
        {
            GetComponentInParent<AirFan>().SetMilliInTrigger(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Milli")
        {
            GetComponentInParent<AirFan>().SetMilliInTrigger(false);
        }
    }

    public bool IsPlayerInXZRange(Vector3 playerPos)
    {
        BoxCollider frontBoxCollider = GetComponent<BoxCollider>();
        Vector3 localPoint = frontBoxCollider.transform.InverseTransformPoint(playerPos);
        Vector3 halfSize = frontBoxCollider.size * 0.5f;

        bool inXZRange = Mathf.Abs(localPoint.x) <= halfSize.x &&
                         Mathf.Abs(localPoint.z) <= halfSize.z;

        return inXZRange;
    }
}
