using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashPendulum : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.collider.IsPlayerCollider())
        {
            // 플레이어 사망 처리
            CharacterBase character = collision.collider.GetComponentInParent<CharacterBase>();
            character.ChangeState<DeadState>();
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
