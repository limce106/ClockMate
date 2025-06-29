using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class ParabolaLaunchStrategy : ILaunchStrategy
{
    private const float MinFallTime = 0.1f;
    private const float VelocityThreshold = 0.1f;

    private Transform target;

    private Coroutine runningCoroutine;
    private MonoBehaviour coroutineRunner;

    private AirFanSetting setting;

    public ParabolaLaunchStrategy(Transform target, AirFanSetting setting, MonoBehaviour coroutineRunner)
    {
        this.target = target;
        this.setting = setting;
        this.coroutineRunner = coroutineRunner;
    }

    public bool CanLaunch(Milli milli, AirFan airFan)
    {
        if (target == null)
            return false;

        if (!airFan.isFanOn)
            return false;

        Transform fanTransform = airFan.transform.Find("Fan");
        if (fanTransform == null)
            return false;

        Vector3 toPlayer = (milli.transform.position - airFan.transform.position).normalized;
        Vector3 fanForward = airFan.transform.forward;

        float dot = Vector3.Dot(fanForward, toPlayer);
        float distance = Vector3.ProjectOnPlane(milli.transform.position - airFan.transform.position, airFan.transform.up).magnitude;

        // 플레이어가 환풍기 앞쪽에 있는지(내적)
        return dot > 0.5f && distance <= setting.launchDistanceThreshold;
    }

    public bool ShouldStopFlying(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        // 환풍기가 꺼지거나 플레이어가 움직이면 비행 중단
        return !airFan.isFanOn || milliRb.velocity.magnitude > VelocityThreshold;
    }

    public void Launch(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        Stop();
        runningCoroutine = coroutineRunner.StartCoroutine(LaunchCoroutine(milli, milliRb, airFan));
    }

    public void Stop()
    {
        if (runningCoroutine != null)
        {
            coroutineRunner.StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }
    }

    public IEnumerator LaunchCoroutine(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
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
            if (ShouldStopFlying(milli, milliRb, airFan))
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
        runningCoroutine = null;
    }
}
