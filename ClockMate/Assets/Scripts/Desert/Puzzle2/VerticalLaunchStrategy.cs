using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalLaunchStrategy : ILaunchStrategy
{
    // 오버슛 방지용, 중력과 물리 힘으로 인한 초과 상승을 방지하기 위해 초기 속도를 줄이는 계수
    private const float overshootPreventFactor = 0.8f;

    public IEnumerator Launch(Milli milli, Rigidbody milliRb, AirFan airFan)
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

        while (airFan.isFanOn && AirFan.isFlying)
        {
            // 환풍기를 벗어나면 바람의 영향을 받지 않는다.
            bool inXZRange = airFan.frontAirFanTrigger.IsPlayerInXZRange(milli.transform.position);
            if (!inXZRange)
            {
                airFan.EndFlying();
                yield break;
            }

            if (milli.transform.position.y >= airFan.windHeight)
            {
                airFan.EndFlying();
                yield break;
            }

            if (!milli.CanJump())
            {
                milli.ResetJumpCount();
            }

            yield return null;
        }

        airFan.EndFlying();
    }
}
