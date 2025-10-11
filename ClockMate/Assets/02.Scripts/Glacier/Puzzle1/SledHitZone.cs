using Photon.Pun;
using UnityEngine;

/// <summary>
/// 썰매 명중 영역(Trigger) 처리
/// </summary>
[RequireComponent(typeof(BoxCollider), typeof(PhotonView))]
public class SledHitZone : MonoBehaviourPun
{
    [SerializeField] private SledHP sledHP;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private string hitSfxKey;

    private void Reset()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (sledHP == null) return;
        
        // 눈덩이 탐색
        var snow = other.GetComponent<Snowball>() ??
                   other.GetComponentInParent<Snowball>();
        if (snow == null) return;

        snow.HitSled(sledHP);
        photonView.RPC(nameof(RPC_HitSledEffect), RpcTarget.All);
        SoundManager.Instance.PlaySfx(key: hitSfxKey, volume: 0.08f, sync: true);
    }
    
    [PunRPC]
    private void RPC_HitSledEffect()
    {
        hitEffect.Play();
    }
}
