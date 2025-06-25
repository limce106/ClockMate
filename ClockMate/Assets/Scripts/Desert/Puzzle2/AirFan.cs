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

    [Header("����")]
    public float windHeight;    // �ٶ��� ����Ǵ� �ִ� ����(������ ����)

    public static bool isFlying = false;   // ���� �÷��̾� ���� ���� �Ϸ� ��, �и��� ���� ���η� �����ϱ�
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;

    [Header("ȯǳ�� ����")]
    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [Header("���������� ����")]
    private ILaunchStrategy launchStrategy;
    private Coroutine parabolaCoroutine;

    [SerializeField, Tooltip("��ǥ ��ġ. ������ ȯǳ�⸸ ������ ��")]
    private Transform target;

    public AirFanSetting setting = new AirFanSetting();

    void Start()
    {
        InitStrategy();
    }

    private void InitStrategy()
    {
        if (launchStrategy != null)
            return;

        Transform fan = transform.Find("Fan");

        if (fan.transform.rotation == Quaternion.identity)
        {
            launchStrategy = new VerticalLaunchStrategy(setting, this);
        }
        else
        {
            launchStrategy = new ParabolaLaunchStrategy(target, setting, this);
        }
    }

    void Update()
    {
        // �׽�Ʈ��
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

        InitMilli();

        if (launchStrategy.CanLaunch(milli, this))
        {
            isFlying = true;
            launchStrategy.Launch(milli, milliRb, this);
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
}
