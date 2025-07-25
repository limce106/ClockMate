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
    public float swingSpeed = 3f;   // 속도

    private bool alreadyTriggered = false;  // DestroyObj를 한 번만 실행하기 위함
    private bool isStarted = false;

    public delegate void PendulumDestroyHandler(Pendulum pendulum);
    public event PendulumDestroyHandler OnPendulumDestroyed;    // 진자가 파괴될 때 실행될 콜백


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        startAngle = transform.localRotation.z;
    }

    /// <summary>
    /// 진자 운동 시작
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
            // 진자가 반대쪽 끝 각에 도달했다면
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
