using Photon.Pun;
using UnityEngine;

public class StageLifeManager : MonoSingleton<StageLifeManager>
{
    [SerializeField] private int stageId;
    [SerializeField] private bool isTest;
    private BoStage _currentStage;
    private CharacterBase _deadCharacter;
    private UIRevive _uiRevive;
    private UIGameOver _uiGameOver;
    private int _reviveCount;
    private int _reviveCounter;

    public void Awake()
    {
        if (isTest)
        {
            _currentStage  = new BoStage(stageId);
        }
        else
        {
            _currentStage = GameManager.Instance.CurrentStage;
        }
        _reviveCount = 10;
        _reviveCounter = 0;
    }

    private void Update()
    {
        if (_deadCharacter != null)
        {
            if (NetworkManager.Instance.IsInRoomAndReady() && !_deadCharacter.photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Revive), RpcTarget.All, _deadCharacter.photonView.ViewID);
                }
                
            }
        }
    }

    public void TryRevive()
    {
        if (++_reviveCounter >= _reviveCount)
        {
            _deadCharacter.transform.position = _currentStage.LoadPositions[_deadCharacter.Name];
            _deadCharacter.ChangeState<IdleState>();

            UIManager.Instance.Close(_uiRevive);
            _deadCharacter = null;
            _reviveCounter = 0;
        }
        else
        {
            _uiRevive.SetProgress(_reviveCounter / (float)_reviveCount);            
        }
        
    }

    public void HandleDeath(CharacterBase deadCharacter)
    {
        if (!NetworkManager.Instance.IsInRoomAndReady() || !deadCharacter.photonView.IsMine) return;

        RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_HandleDeath), RpcTarget.All, deadCharacter.photonView.ViewID);
    }

    public void OnCharacterDeath(CharacterBase deadCharacter)
    {
        _currentStage.ReduceReviveCount();
        Debug.Log($"남은 목숨: {_currentStage.LeftReviveCount}");

        if ((_deadCharacter != null && _deadCharacter != deadCharacter) || _currentStage.IsReviveImpossible)
        {
            _uiGameOver = UIManager.Instance.Show<UIGameOver>("UIGameOver");
            return;
        }

        _deadCharacter = deadCharacter;
        _uiRevive = UIManager.Instance.Show<UIRevive>("UIRevive");
        
        if (_deadCharacter.photonView.IsMine)
            _uiRevive.SetUI(true, _deadCharacter.Name);
        else
            _uiRevive.SetUI(false, _deadCharacter.Name);
    }
}