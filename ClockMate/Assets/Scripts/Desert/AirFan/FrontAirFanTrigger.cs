using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontAirFanTrigger : MonoBehaviour
{
    private const float sizeYOffset = 1f;
    private const float centerYOffset = 0.5f;
    private BoxCollider frontBoxCollider;

    private void Awake()
    {
        frontBoxCollider = GetComponent<BoxCollider>();
    }

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
        Vector3 localPoint = frontBoxCollider.transform.InverseTransformPoint(playerPos);
        Vector3 halfSize = frontBoxCollider.size * 0.5f;

        bool inXZRange = Mathf.Abs(localPoint.x) <= halfSize.x &&
                         Mathf.Abs(localPoint.z) <= halfSize.z;

        return inXZRange;
    }

    public void ExpandFrontTrigger()
    {
        frontBoxCollider.size = new Vector3(frontBoxCollider.size.x, frontBoxCollider.size.y + sizeYOffset, frontBoxCollider.size.z);
        frontBoxCollider.center = new Vector3(frontBoxCollider.center.x, frontBoxCollider.center.y + centerYOffset, frontBoxCollider.center.z);
    }
}
