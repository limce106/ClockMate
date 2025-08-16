using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingClockHand : MonoBehaviourPun
{
    private Rigidbody rb;

    private const float fallForce = 700f;
    private const float lifeTime = 3f;
    private const float stickOffset = 0.2f;

    public delegate void FallingClockHandDisableHandler(GameObject gameObject);
    public event FallingClockHandDisableHandler OnFallingClockHandDisabled;    // �ð� �߰� �ı��� �� ����� �ݹ�

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
        OnFallingClockHandDisabled?.Invoke(gameObject);
        OnFallingClockHandDisabled = null;
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
            BattleManager.Instance.clockhandPool.Return(this);
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
}
