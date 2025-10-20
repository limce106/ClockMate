using Photon.Pun;
using UnityEngine;

/// <summary>
/// 썰매 HP. RPC로 HP 전체 동기화
/// </summary>
public class SledHp : MonoBehaviourPun
{
    [SerializeField] private int maxHp;
    [SerializeField] private ChaseControlModule chaseControl;
    private float _currentHp;
    private UIHpBar _uiHpBar;
    
    public void Init()
    {
        if (_uiHpBar == null)
        {
            _uiHpBar = UIManager.Instance.Show<UIHpBar>("UIHpBar");
        }
        ResetValues();
    }

    public void TakeDamage(float damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _currentHp = Mathf.Max(0, _currentHp - damage);
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHp);

        if (_currentHp <= 0)
        {
            chaseControl.RestartChase();
        }
    }
    
    private void ResetValues()
    {
        _currentHp = maxHp;
        _uiHpBar.UpdateHpBar(maxHp, _currentHp);
    }
    
    [PunRPC] 
    private void RPC_SyncHP(float hp)
    {
        _currentHp = hp;
        _uiHpBar?.UpdateHpBar(maxHp, _currentHp);
    }
}