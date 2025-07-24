using Photon.Pun;
using UnityEngine;

public class SledHP : MonoBehaviourPun
{
    [SerializeField] private int maxHP;
    private int _currentHP;
    private UISledHP _uiSledHP;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _currentHP = maxHP;
        _uiSledHP = UIManager.Instance.Show<UISledHP>("UISledHP");
    }

    public void TakeDamage(int damage)
    {
//        if (!photonView.IsMine) return;

        _currentHP -= damage;
        _uiSledHP.UpdateHpBar(maxHP, _currentHP);
        if (_currentHP <= 0)
        {
            Debug.Log("Game Over");
        }
    }
}