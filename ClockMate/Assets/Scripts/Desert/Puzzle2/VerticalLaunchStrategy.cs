using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalLaunchStrategy : ILaunchStrategy
{
    // ������ ������, �߷°� ���� ������ ���� �ʰ� ����� �����ϱ� ���� �ʱ� �ӵ��� ���̴� ���
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
            // ȯǳ�⸦ ����� �ٶ��� ������ ���� �ʴ´�.
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
