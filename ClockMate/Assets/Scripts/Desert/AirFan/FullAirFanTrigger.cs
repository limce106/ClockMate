using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullAirFanTrigger : MonoBehaviour
{
    private bool isHourInTrigger = false;
    private AirFan airFan;

    void Awake()
    {
        airFan = GetComponentInParent<AirFan>();
    }

    void Update()
    {
        if(isHourInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            airFan.SwitchFan();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Hour")
        {
            isHourInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Hour")
        {
            isHourInTrigger = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Hour")
        {
            isHourInTrigger = true;
        }
    }
}
