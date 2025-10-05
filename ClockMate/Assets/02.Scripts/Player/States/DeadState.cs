using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Battle;

public class DeadState : IState
{
    
    private readonly CharacterBase _character;
    private readonly DeathType _deathType;

    public DeadState(CharacterBase character) =>_character = character;

    public void Enter()
    {
        RPCManager.Instance.photonView.RPC("RPC_SetObjectActive", RpcTarget.All, _character.photonView.ViewID, false);

        if (SceneManager.GetActiveScene().name == "ClockTower")
        {
            if (_deathType == DeathType.None)
            {
                Debug.Log("DeathType is None!");
            }
            else
            {
                BattleLifeManager.Instance.photonView.RPC(nameof(BattleLifeManager.RPC_ReportDeath), RpcTarget.MasterClient, _character.photonView.ViewID);
            }
        }
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        RPCManager.Instance.photonView.RPC("RPC_SetObjectActive", RpcTarget.All, _character.photonView.ViewID, true);
        _character.photonView.RPC("RPC_PlayReviveEffect", RpcTarget.All);
    }
}
