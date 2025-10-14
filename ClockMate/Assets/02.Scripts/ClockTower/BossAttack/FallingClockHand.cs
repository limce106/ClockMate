using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingClockHand : MonoBehaviourPun
{
    private Rigidbody _rb;
    [SerializeField] private Collider _killTrigger;   // 플레이어 죽이는 전용 트리거
    [SerializeField] private Collider _solidCollider; // 물리 충돌용 트리거

    [SerializeField] private ParticleSystem _shockWave;
    [SerializeField] private ParticleSystem _impact;

    private const float fallForce = 1000f;
    private const float lifeTime = 3f;
    private const float stickOffset = 0.03f;

    public delegate void FallingClockHandDisableHandler(GameObject gameObject);
    public event FallingClockHandDisableHandler OnFallingClockHandDisabled;    // 시계 추가 파괴될 때 실행될 콜백

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _killTrigger.enabled = true;
        _solidCollider.enabled = false;

        _rb.isKinematic = false;

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
        _rb.AddForce(Vector3.down * fallForce, ForceMode.Acceleration);
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
    /// 바늘을 땅에 꽂히게 고정하기
    /// </summary>
    private void StickToGround()
    {
        _rb.isKinematic = true;

        transform.position += Vector3.down * stickOffset;

        // 땅에 닿은 바늘로 플레이어가 죽을 수 없도록 처리
        _killTrigger.enabled = false;
        _solidCollider.enabled = true;

        _shockWave.Play();
        _impact.Play();

        SoundManager.Instance.PlaySfx(key: "falling_clockhand", pos: transform.position, volume: 0.7f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StickToGround();
            StartCoroutine(ReturnAfterDelay());
        }
        else
        {
            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            // 상호작용 트리거로 죽지 않도록 플레이어 태그도 검사
            bool canDie = other.IsPlayerCollider() && character != null && _killTrigger.enabled && character.photonView.IsMine;

            if (canDie)
            {
                SoundManager.Instance.PlaySfx(key: "hit", pos: transform.position, volume: 0.7f);

                character.ChangeState<DeadState>(Define.Battle.DeathType.Collision);
            }
        }
    }
}
