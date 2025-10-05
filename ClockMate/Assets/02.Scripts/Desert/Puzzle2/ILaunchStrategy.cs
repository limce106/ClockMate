using System.Collections;
using UnityEngine;

public interface ILaunchStrategy
{
    /// <summary>
    /// 비행 가능 여부
    /// </summary>
    bool CanLaunch(Milli milli, AirFan airFan);
    /// <summary>
    /// 비행 중단 조건 달성 여부
    /// </summary>
    bool ShouldStopFlying(Milli milli, Rigidbody milliRb, AirFan airFan);
    /// <summary>
    /// 플레이어 비행시키기
    /// </summary>
    void Launch(Milli milli, Rigidbody milliRb, AirFan airFan);
    /// <summary>
    /// 플레이어 비행 코루틴
    /// </summary>
    IEnumerator LaunchCoroutine(Milli milli, Rigidbody milliRb, AirFan airFan);
}
