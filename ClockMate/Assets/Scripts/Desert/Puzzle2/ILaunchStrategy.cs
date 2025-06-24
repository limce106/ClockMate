using System.Collections;
using UnityEngine;

public interface ILaunchStrategy
{
    IEnumerator Launch(Milli milli, Rigidbody milliRb, AirFan airFan);
}
