using System;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 눈덩이. 이동은 모든 클라이언트에서 동일하며
/// 명중, 파괴는 마스터 클라이언트가 승인하여 브로드캐스트한다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(SphereCollider))]
public class Snowball : MonoBehaviourPun, ITurretTarget 
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private Transform target;
    
    private ParticleSystem _destroyEffect;

    private bool _isActive;
    
    /// <summary>
    /// 눈덩이가 풀로 반환되거나 비활성화될 때 
    /// </summary>
    public static event Action<ITurretTarget> OnInactive;
    
    private void Awake()
    {
        if (_destroyEffect == null)
        {
            _destroyEffect = Resources.Load<ParticleSystem>("Effects/TargetDestroy");
        }
    }

    public void SetTarget(Transform targetPos)
    {
        target = targetPos;
        _isActive = true;
    }
    
    private void Update()
    {
        if (!_isActive || target is null) return;
        
        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.position += direction.normalized * (speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// SledHitZone에서 마스터 클라이언트만 호출.
    /// HP 감소 + 파괴 브로드캐스트
    /// </summary>
    public void HitSled(SledHP sledHP)
    {
        if (!_isActive) return;
        if (!PhotonNetwork.IsMasterClient) return;
        
        sledHP.TakeDamage(damage);
        SnowballPool.Instance.Return(this);
    }

    /// <summary>
    /// Milli가 눈덩이 맞춘 경우 마스터에게 눈덩이 파괴 승인 요청 보냄
    /// </summary>
    public void OnHit()
    {
        if (!_isActive) return;
        photonView.RPC(nameof(RPC_RequestDestroy), RpcTarget.MasterClient);
    }
    
    [PunRPC] 
    private void RPC_RequestDestroy()
    {
        if (!_isActive) return;
        if (!PhotonNetwork.IsMasterClient) return;
        
        photonView.RPC(nameof(RPC_Destroy), RpcTarget.All);
    }

    [PunRPC] 
    private void RPC_Destroy()
    {
        if (!_isActive) return;
        _isActive = false;

        if (_destroyEffect != null)
        {
            Instantiate(_destroyEffect, transform.position, Quaternion.identity);
        }
        SnowballPool.Instance.Return(this);

    }

    private void OnDisable()
    {
        if (_isActive)
        {
            _isActive = false;
            OnInactive?.Invoke(this);
        }
    }
}