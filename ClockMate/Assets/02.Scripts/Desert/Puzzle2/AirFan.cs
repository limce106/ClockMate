using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AirFan : MonoBehaviourPun
{
    private static Milli _milli;
    private static Rigidbody _milliRb;

    [SerializeField]
    private AirFanBlade _fanBlades;

    [Header("공통")]
    public float windHeight;    // 바람이 적용되는 최대 높이(절대적 높이)

    public static bool isFlying = false;   // 추후 플레이어 날기 구현 완료 시, 밀리의 날기 여부로 변경하기
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;

    [Header("환풍기 상태")]
    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [Header("날려보내기 전략")]
    private ILaunchStrategy _launchStrategy;
    private Coroutine _parabolaCoroutine;

    [SerializeField, Tooltip("목표 위치. 포물선 환풍기만 설정할 것")]
    private Transform _target;

    public AirFanSetting setting = new AirFanSetting();

    void Start()
    {
        InitStrategy();
    }

    private void InitStrategy()
    {
        if (_launchStrategy != null)
            return;

        Transform fan = transform.Find("Fan");

        if (fan.transform.rotation == Quaternion.identity)
        {
            _launchStrategy = new VerticalLaunchStrategy(setting, this);
        }
        else
        {
            _launchStrategy = new ParabolaLaunchStrategy(_target, setting, this);
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

        switch (fanState)
        {
            case FanState.SpinningUp:
                _fanBlades.LerpFanBlades(AirFanBlade.maxRotationSpeed, FanState.Running);
                break;
            case FanState.SpinningDown:
                _fanBlades.LerpFanBlades(0f, FanState.Idle);
                break;
        }
    }

    void FixedUpdate()
    {
        if (fanState != FanState.Running)
            return;

        InitMilli();

        if (_launchStrategy.CanLaunch(_milli, this))
        {
            isFlying = true;
            _launchStrategy.Launch(_milli, _milliRb, this);
        }
    }

    private void InitMilli()
    {
        if (!_milli)
        {
            _milli = FindObjectOfType<Milli>();
            if (_milli != null)
            {
                _milliRb = _milli.GetComponent<Rigidbody>();
            }
        }
    }

    public void EndFlying()
    {
        if (_parabolaCoroutine != null)
        {
            StopCoroutine(_parabolaCoroutine);
            _parabolaCoroutine = null;
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

        _fanBlades.startRotationSpeed = _fanBlades.currentRotationSpeed;
        _fanBlades.startRotationSpeed = _fanBlades.currentRotationSpeed;
        _fanBlades.ClearRotationElapsedTime();
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
}
