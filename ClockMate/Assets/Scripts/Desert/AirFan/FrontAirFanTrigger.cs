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
}
