using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AirFan : MonoBehaviour
{
    private static Milli milli;
    private static Rigidbody milliRb;
    [SerializeField]
    private AirFanBlade fanBlades;

    [Header("�������� ����������")]
    // �ٶ� ����
    [SerializeField]
    private float windForce = 10f;
    // �ٶ��� �ִ� ����
    [SerializeField]
    private float windRange = 5f;
    // �ٶ� Ÿ�� �� �и� �ӵ�
    private float maxHeight;

    [Header("�밢������ ����������")]
    [SerializeField, Tooltip("�밢�� �ٶ��� �� ��ǥ ��ġ�� �����ϼ���.")]
    private Transform targetFlyPoint;

    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [SerializeField]
    private bool isMilliInTrigger = false;
    public static bool isFlying = false;   // ���� �÷��̾� ���� ���� �Ϸ� ��, �и��� ���� ���η� �����ϱ�
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;
    public FrontAirFanTrigger frontAirFanTrigger;

    void Awake()
    {
        if (!milli)
        {
            milli = FindObjectOfType<Milli>();
        }
        if (milli && !milliRb)
        {
            milliRb = milli.GetComponent<Rigidbody>();
        }

        if (transform.rotation == Quaternion.identity)
        {
            isUpwardFly = true;
            maxHeight = transform.position.y + windRange;
        }
        else
        {
            isUpwardFly = false;
        }
    }

    void Update()
    {
        // �׽�Ʈ��
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchFan();
        }
        //

        switch (fanState)
        {
            case FanState.SpinningUp:
                fanBlades.LerpFanBlades(AirFanBlade.maxRotationSpeed, FanState.Running);
                break;
            case FanState.SpinningDown:
                fanBlades.LerpFanBlades(0f, FanState.Idle);
                break;
        }
    }

    void FixedUpdate()
    {
        if (fanState != FanState.Running)
            return;

        if (isUpwardFly)
        {
            if((!isFlying && isMilliInTrigger) || isFlying)
            {
                if(!isFlying)
                {
                    isFlying = true;
                }

                LaunchPlayerUpward();
            }
        }
    }

    private void LaunchPlayerUpward()
    {
        bool inXZRange = frontAirFanTrigger.IsPlayerInXZRange(milli.transform.position);
        if (!inXZRange)
        {
            EndFlying();
            return;
        }

        float distanceToFan = milli.transform.position.y - transform.position.y;

        //if (milli.CurrentStateName == "JumpState")
        //{
        //    return;
        //}

        if (distanceToFan < maxHeight)
        {
            float forceRatio = Mathf.Clamp01(1f - (distanceToFan / maxHeight));
            float upwardForce = forceRatio * windForce;

            milliRb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
        else
        {
            milliRb.velocity = new Vector3(milliRb.velocity.x, 0, milliRb.velocity.z);
        }
    }

    public IEnumerator LaunchPlayerParabola()
    {
        if(!targetFlyPoint)
        {
            yield break;
        }

        isFlying = true;
        milliRb.velocity = Vector3.zero;

        Vector3 start = milli.transform.position;
        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 horizontal = new Vector3(targetFlyPoint.position.x - start.x, 0, targetFlyPoint.position.z - start.z);
        float horizontalDistance = horizontal.magnitude;

        float heightDiff = targetFlyPoint.position.y - start.y;
        float apexHeight = Mathf.Max(windRange, heightDiff + windRange);

        float vy = Mathf.Sqrt(2 * gravity * apexHeight);
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(apexHeight - heightDiff, 0.1f) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 horizontalVelocity = horizontal / totalTime;
        Vector3 launchVelocity = horizontalVelocity + Vector3.up * vy;
        milliRb.velocity = launchVelocity;

        float elapsedTime = 0f;
        while (elapsedTime < totalTime)
        {
            // ȯǳ�Ⱑ �����ų� �÷��̾ �����̸� ���ư��⸦ �����
            if (!isFanOn || milliRb.velocity.magnitude > 0.1f)
            {
                EndFlying();
                yield break;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetFlyPoint.position - start);
            milliRb.MoveRotation(Quaternion.Slerp(milliRb.rotation, targetRotation, Time.deltaTime * 5f));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        milliRb.velocity = Vector3.zero;
        milli.transform.position = targetFlyPoint.position;
        EndFlying();
    }

    public IEnumerator DelayLaunchPlayerParabola(float Delay)
    {
        yield return new WaitForSeconds(Delay);

        yield return LaunchPlayerParabola();
    }

    private void EndFlying()
    {
        //milli.transform.rotation = Quaternion.Euler(Vector3.zero);
        isFlying = false;
    }

    public void SwitchFan()
    {
        if(isFanOn)
        {
            isFanOn = false;
            fanState = FanState.SpinningDown;
            windEffect.Stop();
        }
        else
        {
            isFanOn = true;
            fanState = FanState.SpinningUp;
            windEffect.Play();
        }

        fanBlades.startRotationSpeed = fanBlades.currentRotationSpeed;
        fanBlades.ClearRotationElapsedTime();
    }

    public void SetFanState(FanState nextState)
    {
        fanState = nextState;
    }

    public void SetMilliInTrigger(bool isInTrigger)
    {
        isMilliInTrigger = isInTrigger;
    }
}
