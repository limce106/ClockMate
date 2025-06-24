using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AirFan : MonoBehaviourPun
{
    private static Milli milli;
    private static Rigidbody milliRb;

    [SerializeField]
    private AirFanBlade fanBlades;

    [Header("공통")]
    public float windHeight;    // 바람이 적용되는 최대 높이(절대적 높이)

    [SerializeField]
    private bool isMilliInTrigger = false;
    public static bool isFlying = false;   // 추후 플레이어 날기 구현 완료 시, 밀리의 날기 여부로 변경하기
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;
    public FrontAirFanTrigger frontAirFanTrigger;

    [Header("환풍기 상태")]
    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [Header("날려보내기 전략")]
    private ILaunchStrategy launchStrategy;
    private Coroutine parabolaCoroutine;

    [SerializeField, Tooltip("목표 위치. 포물선 환풍기만 설정할 것")]
    private Transform target;

    void Awake()
    {
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
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
            {
                photonView.RPC("RPC_SwitchFan", RpcTarget.All);
            }
            else
            {
                SwitchFan();
            }
        }
        //

        InitMilli();

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
                    launchStrategy = new VerticalLaunchStrategy();
                    StartCoroutine(launchStrategy.Launch(milli, milliRb, this));
                }
            }
        }
    }

    private void InitMilli()
    {
        if (!milli)
        {
            milli = FindObjectOfType<Milli>();
            if (milli != null)
            {
                milliRb = milli.GetComponent<Rigidbody>();
            }
        }
    }

    public void LaunchParabola()
    {
        if (target == null)
        {
            Debug.LogWarning("목표 위치 설정 안 됨!");
            return;
        }

        if (parabolaCoroutine != null)
        {
            StopCoroutine(parabolaCoroutine);
        }

        launchStrategy = new ParabolaLaunchStrategy(target);
        parabolaCoroutine = StartCoroutine(launchStrategy.Launch(milli, milliRb, this));
        isFlying = true;
    }

    public void EndFlying()
    {
        if (parabolaCoroutine != null)
        {
            StopCoroutine(parabolaCoroutine);
            parabolaCoroutine = null;
        }

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

    [PunRPC]
    public void RPC_SwitchFan()
    {
        SwitchFan();
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
