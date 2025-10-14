using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class SwingPendulum : MonoBehaviourPun
{
    private Rigidbody rb;

    public float startAngle;
    private float swingSpeed = 200f;
    private float torque;
    [SerializeField] private ParticleSystem _sparkEffect;

    private bool alreadyTriggered = false;  // DestroyObj를 한 번만 실행하기 위함
    private bool isStarted = false;

    private const float angleThreshold = 0.5f;

    public delegate void PendulumDisabledHandler(SwingPendulum pendulum);
    public event PendulumDisabledHandler OnPendulumDisabled;    // 시계 추가 파괴될 때 실행될 콜백


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        alreadyTriggered = false;
        isStarted = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        startAngle = NormalizeAngle(transform.eulerAngles.z);
    }

    private void OnDisable()
    {
        OnPendulumDisabled?.Invoke(this);
        OnPendulumDisabled = null;
    }

    /// <summary>
    /// 진자 운동 시작
    /// </summary>
    [PunRPC]
    public void StartPendulum()
    {
        startAngle = NormalizeAngle(transform.eulerAngles.z);
        isStarted = true;
        torque = startAngle < 0 ? swingSpeed : -swingSpeed;

        float sparkEffectRotY = startAngle < 0 ? -90f : 90f;
        _sparkEffect.transform.rotation = Quaternion.Euler(0, sparkEffectRotY, 0);

        StartCoroutine(PlaySwingSfx());
    }

    private IEnumerator PlaySwingSfx()
    {
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySfx(key: "swing_pendulum", pos: transform.position, volume: 0.7f);
    }

    private void FixedUpdate()
    {
        if (!isStarted)
            return;

        rb.AddTorque(Vector3.forward * torque, ForceMode.Force);
    }

    private void Update()
    {
        if (!isStarted)
            return;

        if (!alreadyTriggered)
        {
            float zAngle = NormalizeAngle(transform.localEulerAngles.z);
            float targetAngle = -startAngle;

            // 진자가 반대쪽 끝 각에 도달했다면
            if (Mathf.Abs(zAngle - targetAngle) < angleThreshold)
            {
                alreadyTriggered = true;

                if (PhotonNetwork.IsMasterClient)
                    BattleManager.Instance.pendulumPool.Return(this);
            }
        }
    }

    float NormalizeAngle(float angle)
    {
        return (angle + 180) % 360 - 180;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CharacterBase character = collision.collider.GetComponentInParent<CharacterBase>();
        if (character != null)
        {
            SoundManager.Instance.PlaySfx(key: "hit", pos: transform.position, volume: 0.7f);

            character.ChangeState<DeadState>(Define.Battle.DeathType.Collision);
        }
    }
}
