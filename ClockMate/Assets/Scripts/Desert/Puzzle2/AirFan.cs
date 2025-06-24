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

    [SerializeField]
    private bool isMilliInTrigger = false;
    public static bool isFlying = false;   // ���� �÷��̾� ���� ���� �Ϸ� ��, �и��� ���� ���η� �����ϱ�
    public bool isUpwardFly = false;

    public ParticleSystem windEffect;
    public FrontAirFanTrigger frontAirFanTrigger;

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
            Debug.LogWarning("��ǥ ��ġ ���� �� ��!");
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
