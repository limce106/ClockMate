using Photon.Pun;
using UnityEngine;

/// <summary>
/// 썰매 HP. RPC로 HP 전체 동기화
/// </summary>
public class SledHP : MonoBehaviourPun
{
    [SerializeField] private int maxHP;
    [SerializeField] private SledChaseOrchestrator orchestrator;
    private int _currentHP;
    private UISledHP _uiSledHP;
    
    public void Init()
    {
        _currentHP = maxHP;
        _uiSledHP = UIManager.Instance.Show<UISledHP>("UISledHP");
        _uiSledHP.UpdateHpBar(maxHP, _currentHP);
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _currentHP = Mathf.Max(0, _currentHP - damage);
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHP);

        if (_currentHP <= 0)
        {
            orchestrator.RequestReStart();
        }
    }
    
    [PunRPC] 
    private void RPC_SyncHP(int hp)
    {
        _currentHP = hp;
        _uiSledHP?.UpdateHpBar(maxHP, _currentHP);
    }

}