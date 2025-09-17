using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Battle;

public class DeadState : IState
{
    
    private readonly CharacterBase _character;
    private readonly DeathType _deathType;

    public DeadState(CharacterBase character) =>_character = character;
    public DeadState(CharacterBase character, DeathType deathType)
    {
        _character = character;
        _deathType = deathType;
    }

    public void Enter()
    {
        RPCManager.Instance.photonView.RPC("RPC_SetObjectActive", Photon.Pun.RpcTarget.All, _character.photonView.ViewID, false);

        if (SceneManager.GetActiveScene().name == "ClockTower")
        {
            if (_deathType == DeathType.None)
            {
                Debug.Log("DeathType is None!");
            }
            else
            {
                BattleLifeManager.Instance.HandleDeath(_character, _deathType);
            }
        }
        else
        {
            PuzzleLifeManager.Instance.HandleDeath(_character);
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
        RPCManager.Instance.photonView.RPC("RPC_SetObjectActive", Photon.Pun.RpcTarget.All, _character.photonView.ViewID, true);
        _character.photonView.RPC("RPC_PlayReviveEffect", Photon.Pun.RpcTarget.All);
    }
}
