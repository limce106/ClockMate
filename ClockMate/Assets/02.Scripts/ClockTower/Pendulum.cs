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
    public float swingSpeed = 3f;   // �ӵ�

    private bool alreadyTriggered = false;  // DestroyObj�� �� ���� �����ϱ� ����
    private bool isStarted = false;

    public delegate void PendulumDestroyHandler(Pendulum pendulum);
    public event PendulumDestroyHandler OnPendulumDestroyed;    // ���ڰ� �ı��� �� ����� �ݹ�


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        startAngle = transform.localRotation.z;
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

        float torque = startAngle < 0 ? 20f : -20f;
        GetComponent<Rigidbody>().AddTorque(Vector3.forward * torque, ForceMode.Impulse);
    }

    private void Update()
    {
        if (!isStarted)
            return;

        float zAngle = NormalizeAngle(transform.localEulerAngles.z);

        if(!alreadyTriggered)
        {
            // ���ڰ� �ݴ��� �� ���� �����ߴٸ�
            if(startAngle < 0 && zAngle > -startAngle ||
                startAngle > 0 && zAngle < -startAngle)
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
