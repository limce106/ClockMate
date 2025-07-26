using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class Pendulum : MonoBehaviourPun
{
    private Rigidbody rb;

    public float startAngle;
    private float swingSpeed = 50;

    private bool alreadyTriggered = false;  // DestroyObj�� �� ���� �����ϱ� ����
    private bool isStarted = false;

    private const float angleThreshold = 0.5f;

    public delegate void PendulumDestroyHandler(Pendulum pendulum);
    public event PendulumDestroyHandler OnPendulumDestroyed;    // ���ڰ� �ı��� �� ����� �ݹ�


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startAngle = NormalizeAngle(transform.eulerAngles.z);
    }

    /// <summary>
    /// ���� � ����
    /// </summary>
    [PunRPC]
    public void StartPendulum()
    {
        if (isStarted)
            return;
        isStarted = true;

        float torque = startAngle < 0 ? swingSpeed : -swingSpeed;
        rb.AddTorque(Vector3.forward * torque, ForceMode.Impulse);
    }

    private void Update()
    {
        if (!isStarted)
            return;

        if(!alreadyTriggered)
        {
            float zAngle = NormalizeAngle(transform.localEulerAngles.z);
            float targetAngle = -startAngle;

            // ���ڰ� �ݴ��� �� ���� �����ߴٸ�
            if(Mathf.Abs(zAngle - targetAngle) < angleThreshold)
            {
                alreadyTriggered = true;
                DestroyObj();
            }
        }
    }

    float NormalizeAngle(float angle)
    {
        return (angle + 180) % 360 - 180;
    }

    public void DestroyObj()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnPendulumDestroyed.Invoke(this);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
