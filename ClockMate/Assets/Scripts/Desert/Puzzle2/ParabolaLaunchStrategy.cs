using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

[CreateAssetMenu(menuName = "AirFan/ParabolaLaunchStrategy")]
public class ParabolaLaunchStrategy : ILaunchStrategy
{
    private const float MinFallTime = 0.1f;
    private const float VelocityThreshold = 0.1f;

    private Transform target;

    public ParabolaLaunchStrategy(Transform target)
    {
        this.target = target;
    }

    public IEnumerator Launch(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        if(target == null)
        {
            Debug.LogWarning("ParabolaLaunchStrategySO: 타겟 설정 필요!");
            airFan.EndFlying();
            yield break;
        }

        milliRb.velocity = Vector3.zero;
        Vector3 start = milli.transform.position;
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 horizontal = new Vector3(target.position.x - start.x, 0, target.position.z - start.z);

        float heightDiff = target.position.y - start.y;
        float apexHeight = Mathf.Max(airFan.windHeight, heightDiff + airFan.windHeight);

        float vy = Mathf.Sqrt(2 * gravity * apexHeight);
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(apexHeight - heightDiff, MinFallTime) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 horizontalVelocity = horizontal / totalTime;
        Vector3 launchVelocity = horizontalVelocity + Vector3.up * vy;
        milliRb.velocity = launchVelocity;

        Vector3 lookDir = target.position - start;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            milli.transform.forward = lookDir.normalized;
        }

        float elapsedTime = 0f;
        while (elapsedTime < totalTime)
        {
            // 환풍기가 꺼지거나 플레이어가 움직이면 날아가기를 멈춘다
            if (!airFan.isFanOn || milliRb.velocity.magnitude > VelocityThreshold)
            {
                airFan.EndFlying();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        milliRb.velocity = Vector3.zero;
        milli.transform.position = target.position;
        airFan.EndFlying();
    }
}
