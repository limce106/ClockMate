using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using System;
using static Define.Character;

public class RPCManager : MonoBehaviourPunCallbacks
{
    private static RPCManager _instance;
    public static RPCManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = FindObjectOfType<RPCManager>();
                if (obj != null)
                    _instance = obj;
            }
            return _instance;
        }
    }

    private static Dictionary<int, bool> _playerReadyStatus = new Dictionary<int, bool>();
    private bool _canAcceptReady = false;

    public static Action OnLocalAllReadyAction;
    public static Action OnSyncedAllReadyAction;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom || !_canAcceptReady)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            bool isReady = false;
            _playerReadyStatus.TryGetValue(actorNumber, out isReady);
            if (isReady)
            {
                photonView.RPC("UnmarkReady", RpcTarget.MasterClient, actorNumber);
            }
            else
            {
                photonView.RPC("MarkReady", RpcTarget.MasterClient, actorNumber);
            }
        }
    }

    [PunRPC]
    public void SetCanAcceptReady(bool value)
    {
        _canAcceptReady = value;
    }

    [PunRPC]
    void MarkReady(int actorNumber)
    {
        photonView.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, true);
        TryExecuteOnAllPlayersReady();
        Debug.Log("준비 완료");
    }

    [PunRPC]
    void UnmarkReady(int actorNumber)
    {
        photonView.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, false);
        Debug.Log("준비 해제");
    }

    // playerReadyStatus는 반드시 SyncReadyStatus를 통해서만 수정할 것
    [PunRPC]
    void SyncReadyStatus(int actorNumber, bool isReady)
    {
        _playerReadyStatus[actorNumber] = isReady;
    }

    [PunRPC]
    private void ResetReadyState()
    {
        _playerReadyStatus.Clear();
        _canAcceptReady = false;
    }

    void TryExecuteOnAllPlayersReady()
    {
        if (!AllPlayersReady())
            return;

        OnLocalAllReadyAction?.Invoke();
        OnLocalAllReadyAction = null;

        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ExecuteSyncedAllPlayersReady", RpcTarget.All);
            photonView.RPC("ResetReadyState", RpcTarget.All);
        }
    }

    [PunRPC]
    private void ExecuteSyncedAllPlayersReady()
    {
        OnSyncedAllReadyAction?.Invoke();
        OnSyncedAllReadyAction = null;
    }

    bool AllPlayersReady()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = player.Value.ActorNumber;

            if(!_playerReadyStatus.ContainsKey(actorNumber) || !_playerReadyStatus[actorNumber])
                return false;
        }
        return true;
    }

    public static Dictionary<int, bool> GetPlayerReadyStatus()
    {
        // 복사 반환하여 원본에 영향 없도록 함
        return new Dictionary<int, bool>(_playerReadyStatus);
    }

    [PunRPC]
    public void ResetAllReadyStates()
    {
        ResetReadyState();
        
        foreach(var plyaer in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = plyaer.Value.ActorNumber;
            photonView.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, false);
        }
    }

    [PunRPC]
    public void DeleteAllSaveData()
    {
        SaveManager.Instance?.DeleteSaveData();
    }

    [PunRPC]
    public void RPC_RegisterCharacter(CharacterName character, int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            CharacterBase characterBase = pv.GetComponent<CharacterBase>();
            if (characterBase != null)
            {
                GameManager.Instance.RegisterCharacter(character, characterBase);
            }
            else
            {
                Debug.LogError($"[RPC_RegisterCharacter] CharacterBase 컴포넌트를 찾을 수 없음, ViewID: {viewID}");
            }
        }
        else
        {
            Debug.LogError($"[RPC_RegisterCharacter] PhotonView를 찾을 수 없음, ViewID: {viewID}");
        }
    }
}
