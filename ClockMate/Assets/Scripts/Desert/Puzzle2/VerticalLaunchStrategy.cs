using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class VerticalLaunchStrategy : ILaunchStrategy
{
    // 오버슛 방지용, 중력과 물리 힘으로 인한 초과 상승을 방지하기 위해 초기 속도를 줄이는 계수
    private const float overshootPreventFactor = 0.8f;

    private MonoBehaviour coroutineRunner;
    private AirFanSetting setting;

    public VerticalLaunchStrategy(AirFanSetting setting, MonoBehaviour coroutineRunner)
    {
        this.setting = setting;
        this.coroutineRunner = coroutineRunner;
    }

    public bool CanLaunch(Milli milli, AirFan airFan)
    {
        if(milli == null)
        {
            return false;
        }

        if (!airFan.isFanOn)
            return false;

        // 환풍기를 벗어나면 바람의 영향을 받지 않는다.
        bool inXZRange = IsPlayerInXZRange(milli.transform.position, airFan);


        return inXZRange && milli.transform.position.y <= airFan.transform.position.y + setting.launchDistanceThreshold;
    }

    public bool ShouldStopFlying(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        if (!airFan.isFanOn)
            return true;

        if (!IsPlayerInXZRange(milli.transform.position, airFan))
            return true;

        if (milli.transform.position.y >= airFan.windHeight)
            return true;

        return false;
    }

    public void Launch(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        coroutineRunner.StartCoroutine(LaunchCoroutine(milli, milliRb, airFan));
    }

    public IEnumerator LaunchCoroutine(Milli milli, Rigidbody milliRb, AirFan airFan)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float remainingHeight = airFan.windHeight - airFan.transform.position.y;

        if(remainingHeight <= 0 )
        {
            airFan.EndFlying();
            yield break;
        }

        float initialVelocity = Mathf.Sqrt(2 * gravity * remainingHeight) * overshootPreventFactor;
        milliRb.velocity = new Vector3(milliRb.velocity.x, initialVelocity, milliRb.velocity.z);

        while (!ShouldStopFlying(milli, milliRb, airFan))
        {
            if (!milli.CanJump())
            {
                coroutineRunner.StartCoroutine(ResetJumpCountAfterDelay(milli, 1f));
            }

            yield return null;
        }

        airFan.EndFlying();
    }

    public bool IsPlayerInXZRange(Vector3 playerPos, AirFan airFan)
    {
        // 환풍기의 로컬 좌표계로 변환
        Vector3 localPoint = airFan.transform.InverseTransformPoint(playerPos);

        // 환풍기 본체의 절반 크기
        Vector3 halfSize = new Vector3(0.5f, 0, 0.5f);

        bool inXZRange =
            Mathf.Abs(localPoint.x) <= halfSize.x &&
            Mathf.Abs(localPoint.z) <= halfSize.z;

        return inXZRange;
    }

    private IEnumerator ResetJumpCountAfterDelay(Milli milli, float delay)
    {
        yield return new WaitForSeconds(delay);
        milli.ResetJumpCount();
    }
}
