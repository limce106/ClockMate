using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCPressurePlate : MonoBehaviour, IDoorCondition
{
    [SerializeField] private PressurePlate pressurePlate;

    public bool IsConditionMet()
    {
        return pressurePlate.IsFullyPressed;
    }
}
