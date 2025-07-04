using System.Collections;
using UnityEngine;

public interface ILaunchStrategy
{
    bool CanLaunch(Milli milli, AirFan airFan);
    bool ShouldStopFlying(Milli milli, Rigidbody milliRb, AirFan airFan);
    void Launch(Milli milli, Rigidbody milliRb, AirFan airFan);
    IEnumerator LaunchCoroutine(Milli milli, Rigidbody milliRb, AirFan airFan);
}
