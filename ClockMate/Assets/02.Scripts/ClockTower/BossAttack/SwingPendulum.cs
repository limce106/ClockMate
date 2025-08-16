using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class SwingPendulum : MonoBehaviourPun
{
    private Rigidbody rb;

    public float startAngle;
    private float swingSpeed = 45f;
    private float torque;

    private bool alreadyTriggered = false;  // DestroyObj�� �� ���� �����ϱ� ����
    private bool isStarted = false;

    private const float angleThreshold = 0.5f;

    public delegate void PendulumDisabledHandler(SwingPendulum pendulum);
    public event PendulumDisabledHandler OnPendulumDisabled;    // �ð� �߰� �ı��� �� ����� �ݹ�


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
    /// ���� � ����
    /// </summary>
    [PunRPC]
    public void StartPendulum()
    {
        startAngle = NormalizeAngle(transform.eulerAngles.z);
        isStarted = true;
        torque = startAngle < 0 ? swingSpeed : -swingSpeed;
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

            // ���ڰ� �ݴ��� �� ���� �����ߴٸ�
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(collision.collider.IsPlayerCollider())
        {
            // �÷��̾� ��� ó��
            CharacterBase character = collision.collider.GetComponentInParent<CharacterBase>();
            character.ChangeState<DeadState>();
        }
    }
}
