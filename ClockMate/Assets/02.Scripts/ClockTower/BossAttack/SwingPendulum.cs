using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class SwingPendulum : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;

    public float startAngle;
    private float swingSpeed = 45f;
    private float torque;

    private bool alreadyTriggered = false;  // DestroyObj를 한 번만 실행하기 위함
    private bool isStarted = false;

    private const float angleThreshold = 0.5f;

    public delegate void PendulumDestroyHandler(SwingPendulum pendulum);
    public event PendulumDestroyHandler OnPendulumDestroyed;    // 시계 추가 파괴될 때 실행될 콜백


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        startAngle = NormalizeAngle(transform.eulerAngles.z);
    }

    /// <summary>
    /// 진자 운동 시작
    /// </summary>
    [PunRPC]
    public void StartPendulum()
    {
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

            // 진자가 반대쪽 끝 각에 도달했다면
            if (Mathf.Abs(zAngle - targetAngle) < angleThreshold)
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

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(collision.collider.IsPlayerCollider())
        {
            // 플레이어 사망 처리
            CharacterBase character = collision.collider.GetComponentInParent<CharacterBase>();
            
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            Quaternion rotation = (Quaternion)stream.ReceiveNext();
            Vector3 velocity = (Vector3)stream.ReceiveNext();
            Vector3 angularVelocity = (Vector3)stream.ReceiveNext();

            rb.position = Vector3.Lerp(rb.position, position, Time.deltaTime * 10f);
            rb.rotation = Quaternion.Slerp(rb.rotation, rotation, Time.deltaTime * 10f);

            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
        }
    }
}
