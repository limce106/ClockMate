using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 썰매 HP. RPC로 HP 전체 동기화
/// </summary>
public class SledHp : MonoBehaviourPun
{
    [SerializeField] private int maxHp;
    [SerializeField] private SledChaseOrchestrator orchestrator;
    private int _currentHp;
    private UIHpBar _uiHpBar;
    
    public void Init()
    {
        _currentHp = maxHp;
        _uiHpBar = UIManager.Instance.Show<UIHpBar>("UIHpBar");
        _uiHpBar.UpdateHpBar(maxHp, _currentHp);
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _currentHp = Mathf.Max(0, _currentHp - damage);
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHp);

        if (_currentHp <= 0)
        {
            orchestrator.RequestRestart();
        }
    }
    
    [PunRPC] 
    private void RPC_SyncHP(int hp)
    {
        _currentHp = hp;
        _uiHpBar?.UpdateHpBar(maxHp, _currentHp);
    }
}