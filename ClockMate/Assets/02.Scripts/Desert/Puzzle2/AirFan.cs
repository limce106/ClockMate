using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AirFan : MonoBehaviourPun, IPunObservable
{
    private static Milli _milli;
    private static Rigidbody _milliRb;

    [SerializeField]
    private AirFanBlade _fanBlades;

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
    private ILaunchStrategy _launchStrategy;
    private Coroutine _parabolaCoroutine;

    [SerializeField, Tooltip("��ǥ ��ġ. ������ ȯǳ�⸸ ������ ��")]
    private Transform _target;

    public AirFanSetting setting = new AirFanSetting();

    [Header("ȿ����")] 
    [SerializeField] private string onSfxKey = "fan_on";
    [SerializeField] private string runningSfxKey= "fan_noise";
    [SerializeField] private string offSfxKey = "fan_stop";
    [SerializeField] private float sfxVolume = 0.8f;
    
    private SoundHandle _runningSfxHandle;

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
        // �׽�Ʈ��
        if (Input.GetKeyDown(KeyCode.K))
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
            _milli.Anim.SetFanFly(true);
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
            SoundManager.Instance.Stop(_runningSfxHandle);
            SoundManager.Instance.PlaySfx(key: offSfxKey, pos: transform.position, volume: sfxVolume);
        }
        else
        {
            isFanOn = true;
            fanState = FanState.SpinningUp;
            windEffect.Play();
            SoundManager.Instance.PlaySfx(key: onSfxKey, pos: transform.position, volume: sfxVolume);
            _runningSfxHandle = SoundManager.Instance.PlaySfx(key: runningSfxKey, pos: transform.position, volume: sfxVolume, loop: true, delay: 0.7f);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFanOn);
            stream.SendNext(fanState);
        }
        else
        {
            bool remoteIsFanOn = (bool)stream.ReceiveNext();
            FanState remoteFanState = (FanState)stream.ReceiveNext();

            // ���� ���°� ���� ���� ���¿� �ٸ��� ������ ����ȭ
            if (remoteIsFanOn != isFanOn)
            {
                isFanOn = remoteIsFanOn;

                if (isFanOn)
                {
                    fanState = FanState.SpinningUp;
                    windEffect.Play();
                }
                else
                {
                    fanState = FanState.SpinningDown;
                    windEffect.Stop();
                }
            }

            // fanState�� ����ȭ���� �ʾ��� ��쿡�� ó��
            if (remoteFanState != fanState)
            {
                fanState = remoteFanState;
            }
        }
    }
}
