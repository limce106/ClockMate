using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AirFan : MonoBehaviour
{
    private Milli milli;
    private Rigidbody milliRb;
    [SerializeField]
    private AirFanBlade fanBlades;

    [Header("수직으로 날려보내기")]
    // 바람 세기
    [SerializeField]
    private float windForce = 10f;
    // 바람의 최대 범위
    [SerializeField]
    private float windRange = 5f;

    [Header("대각선으로 날려보내기")]
    [SerializeField, Tooltip("대각선 바람일 때 목표 위치를 설정하세요.")]
    private Transform targetFlyPoint;
    private const float gravity = 9.8f;

    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [SerializeField]
    private bool isMilliInTrigger = false;
    [SerializeField]
    private bool isFlying = false;
    private bool isUpwardFly = false;

    public ParticleSystem windEffect;

    public FrontAirFanTrigger frontAirFanTrigger;
    void Start()
    {
        milli = FindObjectOfType<Milli>();
        if (milli != null )
        {
            milliRb = milli.GetComponent<Rigidbody>();

            if (Mathf.Approximately(transform.rotation.eulerAngles.x, 0f))
            {
                isUpwardFly = true;
            }
            else
            {
                isUpwardFly = false;
                
            }
        }
    }

    void Update()
    {
        // 테스트용
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
        else
        {
            StartCoroutine(DelayLaunchPlayerParabola(0.7f));
        }
    }

    private void LaunchPlayerUpward()
    {
        bool inXZRange = frontAirFanTrigger.IsPlayerInXZRange(milli.transform.position);
        if (!inXZRange)
            return;

        float distanceToFan = milli.transform.position.y - transform.position.y;

        //if (milli.CurrentStateName == "JumpState")
        //{
        //    return;
        //}

        if (distanceToFan < windRange)
        {
            float forceRatio = Mathf.Clamp01(1f - (distanceToFan / windRange));
            float upwardForce = forceRatio * windForce;

            milliRb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
        else
        {
            milliRb.velocity = new Vector3(milliRb.velocity.x, 0, milliRb.velocity.z);
        }
    }

    private IEnumerator LaunchPlayerParabola()
    {
        isFlying = true;
        milliRb.useGravity = false;

        Transform startFlyPoint = milli.transform;
        float distance = Vector3.Distance(startFlyPoint.position, targetFlyPoint.position);
        float flyAngle = 90f - transform.rotation.eulerAngles.x;

        float veloctiy = distance / (Mathf.Sin(2 * flyAngle * Mathf.Deg2Rad) / gravity);
        float Vx = Mathf.Sqrt(veloctiy) * Mathf.Cos(flyAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(veloctiy) * Mathf.Sin(flyAngle * Mathf.Deg2Rad);

        float flightDuration = distance / Vx;
        Quaternion targetRotation = Quaternion.LookRotation(targetFlyPoint.position - startFlyPoint.position);

        Vector3 startPosition = milli.transform.position;
        float elapseTime = 0f;
        while (elapseTime < flightDuration)
        {
            // 플레이어가 수동 조작으로 움직인 경우 낙하
            if (!isFanOn || milliRb.velocity.magnitude  > 0.1f)
            {
                Debug.Log("Player moved: Cancel Flying");

                EndFlying();

                yield break;
            }

            float timeRatio = elapseTime / flightDuration;

            float yOffset = (Vy * elapseTime) - (0.5f * gravity * elapseTime * elapseTime);
            Vector3 horizontalMovement = Vector3.forward * Vx * elapseTime;
            Vector3 newPosition = startPosition + milli.transform.TransformDirection(horizontalMovement) + Vector3.up * yOffset;

            milli.transform.position = newPosition;
            milli.transform.rotation = Quaternion.Slerp(milli.transform.rotation, targetRotation, Time.deltaTime * 5f);

            elapseTime += Time.deltaTime;
            yield return null;
        }

        milli.transform.position = targetFlyPoint.position;
        EndFlying();
    }

    private IEnumerator DelayLaunchPlayerParabola(float Delay)
    {
        yield return new WaitForSeconds(Delay);

        if (!isFlying && isMilliInTrigger && isFanOn)
        {
            yield return LaunchPlayerParabola();
        }
    }

    private void EndFlying()
    {
        milli.transform.rotation = Quaternion.Euler(Vector3.zero);

        isFlying = false;
        milliRb.useGravity = true;
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
