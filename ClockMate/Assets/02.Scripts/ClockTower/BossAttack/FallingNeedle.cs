using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingNeedle : MonoBehaviourPun
{
    private Rigidbody rb;

    private const float fallForce = 500f;
    private const float lifeTime = 3f;
    private const float stickOffset = 0.2f;

    public delegate void FallingNeedleDisableHandler(GameObject gameObject);
    public event FallingNeedleDisableHandler OnFallingNeedleDisabled;    // �ð� �߰� �ı��� �� ����� �ݹ�

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(ApplyFallForce), RpcTarget.All);
    }

    private void OnDisable()
    {
        OnFallingNeedleDisabled?.Invoke(gameObject);
        OnFallingNeedleDisabled = null;
    }

    [PunRPC]
    void ApplyFallForce()
    {
        rb.AddForce(Vector3.down * fallForce, ForceMode.Acceleration);
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);

        if (PhotonNetwork.IsMasterClient)
        {
            BattleManager.Instance.needlePool.Return(this);
        }
    }

    /// <summary>
    /// �ٴ��� ���� ������ �����ϱ�
    /// </summary>
    private void StickToGround()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position += Vector3.down * stickOffset;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StickToGround();
            StartCoroutine(ReturnAfterDelay());
        }

        if (collision.collider.IsPlayerCollider())
        {
            // �÷��̾� ��� ó��
            CharacterBase character = collision.collider.GetComponentInParent<CharacterBase>();
            character.ChangeState<DeadState>();
        }
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(transform.position);
    //        stream.SendNext(rb.rotation);
    //        stream.SendNext(rb.velocity);
    //        stream.SendNext(rb.angularVelocity);
    //    }
    //    else
    //    {
    //        Vector3 position = (Vector3)stream.ReceiveNext();
    //        Quaternion rotation = (Quaternion)stream.ReceiveNext();
    //        Vector3 velocity = (Vector3)stream.ReceiveNext();
    //        Vector3 angularVelocity = (Vector3)stream.ReceiveNext();

    //        transform.position = position;
    //        //rb.position = Vector3.Lerp(rb.position, position, Time.deltaTime * 10f);
    //        rb.rotation = Quaternion.Slerp(rb.rotation, rotation, Time.deltaTime * 10f);

    //        rb.velocity = velocity;
    //        rb.angularVelocity = angularVelocity;
    //    }
    //}
}
