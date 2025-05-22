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

    [Header("공통")]
    // 바람이 적용되는 최대 높이(절대적 높이)
    public float windHeight;
    [SerializeField]
    private bool isMilliInTrigger = false;
    public static bool isFlying = false;   // 추후 플레이어 날기 구현 완료 시, 밀리의 날기 여부로 변경하기
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;
    public FrontAirFanTrigger frontAirFanTrigger;

    [Header("수직으로 날려보내기")]
    // 오버슛 방지용, 중력과 물리 힘으로 인한 초과 상승을 방지하기 위해 초기 속도를 줄이는 계수
    private const float overshootPreventFactor = 0.8f;

    [Header("대각선으로 날려보내기")]
    [SerializeField, Tooltip("대각선 바람일 때 목표 위치를 설정하세요.")]
    private Transform targetFlyPoint;
    private const float MinFallTime = 0.1f;
    private const float VelocityThreshold = 0.1f;
    private const float RotationSpeedWhileFlying = 5f;

    [Header("환풍기 상태")]
    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

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
        }
        else
        {
            isUpwardFly = false;
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
            if ((!isFlying && isMilliInTrigger) || isFlying)
            {
                if (!isFlying)
                {
                    isFlying = true;
                }

                LaunchPlayerUpward();
            }
        }
    }

    private void LaunchPlayerUpward()
    {
        // 환풍기를 벗어나면 바람의 영향을 받지 않는다.
        bool inXZRange = frontAirFanTrigger.IsPlayerInXZRange(milli.transform.position);
        if (!inXZRange)
        {
            EndFlying();
            return;
        }

        float currentHeight = milli.transform.position.y;
        float remainingHeight = windHeight - transform.position.y;

        if (remainingHeight > 0f)
        {
            float gravity = Mathf.Abs(Physics.gravity.y);
            float gravityForce = milliRb.mass * gravity;
            float initialVelocity = Mathf.Sqrt(2 * gravity * remainingHeight) * overshootPreventFactor;

            milliRb.velocity = new Vector3(milliRb.velocity.x, initialVelocity, milliRb.velocity.z);

            isFlying = true;
        }
        //else
        //{
        //    milliRb.velocity = new Vector3(milliRb.velocity.x, 0f, milliRb.velocity.z);
        //    milliRb.AddForce(-Physics.gravity * milliRb.mass);
        //}

        if (!milli.CanJump())
        {
            milli.ResetJumpCount();
        }
    }

    public IEnumerator LaunchPlayerParabola()
    {
        if (!targetFlyPoint)
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
        float apexHeight = Mathf.Max(windHeight, heightDiff + windHeight);

        float vy = Mathf.Sqrt(2 * gravity * apexHeight);
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(apexHeight - heightDiff, MinFallTime) / gravity);
        float totalTime = timeUp + timeDown;

        Vector3 horizontalVelocity = horizontal / totalTime;
        Vector3 launchVelocity = horizontalVelocity + Vector3.up * vy;
        milliRb.velocity = launchVelocity;

        float elapsedTime = 0f;
        while (elapsedTime < totalTime)
        {
            // 환풍기가 꺼지거나 플레이어가 움직이면 날아가기를 멈춘다
            if (!isFanOn || milliRb.velocity.magnitude > VelocityThreshold)
            {
                EndFlying();
                yield break;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetFlyPoint.position - start);
            milliRb.MoveRotation(Quaternion.Slerp(milliRb.rotation, targetRotation, Time.deltaTime * RotationSpeedWhileFlying));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        milliRb.velocity = Vector3.zero;
        milli.transform.position = targetFlyPoint.position;
        EndFlying();
    }

    private void EndFlying()
    {
        isFlying = false;
    }

    public void SwitchFan()
    {
        if (isFanOn)
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
