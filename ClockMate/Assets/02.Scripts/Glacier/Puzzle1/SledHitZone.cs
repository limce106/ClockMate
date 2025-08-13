using Photon.Pun;
using UnityEngine;

/// <summary>
/// 썰매 명중 영역(Trigger) 처리
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class SledHitZone : MonoBehaviour
{
    [SerializeField] private SledHP sledHP;

    private void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.4f;
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
    }
}
