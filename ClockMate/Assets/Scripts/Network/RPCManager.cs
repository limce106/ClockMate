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
    private static RPCManager instance;
    public static RPCManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<RPCManager>();
                if (obj != null)
                    instance = obj;
            }
            return instance;
        }
    }

    private PhotonView PV;
    private static Dictionary<int, bool> playerReadyStatus = new Dictionary<int, bool>();
    private bool canAcceptReady = false;

    public static Action OnLocalAllReadyAction;
    public static Action OnSyncedAllReadyAction;

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom || !canAcceptReady)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            bool isReady = false;
            playerReadyStatus.TryGetValue(actorNumber, out isReady);
            if (isReady)
            {
                PV.RPC("UnmarkReady", RpcTarget.MasterClient, actorNumber);
            }
            else
            {
                PV.RPC("MarkReady", RpcTarget.MasterClient, actorNumber);
            }
        }
    }

    [PunRPC]
    public void SetCanAcceptReady(bool value)
    {
        canAcceptReady = value;
    }

    [PunRPC]
    void MarkReady(int actorNumber)
    {
        PV.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, true);
        TryExecuteOnAllPlayersReady();
        Debug.Log("�غ� �Ϸ�");
    }

    [PunRPC]
    void UnmarkReady(int actorNumber)
    {
        PV.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, false);
        Debug.Log("�غ� ����");
    }

    // playerReadyStatus�� �ݵ�� SyncReadyStatus�� ���ؼ��� ������ ��
    [PunRPC]
    void SyncReadyStatus(int actorNumber, bool isReady)
    {
        playerReadyStatus[actorNumber] = isReady;
    }

    [PunRPC]
    private void ResetReadyState()
    {
        playerReadyStatus.Clear();
        canAcceptReady = false;
    }

    void TryExecuteOnAllPlayersReady()
    {
        if (!AllPlayersReady())
            return;

        OnLocalAllReadyAction?.Invoke();
        OnLocalAllReadyAction = null;

        if(PhotonNetwork.IsMasterClient)
        {
            PV.RPC("ExecuteSyncedAllPlayersReady", RpcTarget.All);
            PV.RPC("ResetReadyState", RpcTarget.All);
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

            if(!playerReadyStatus.ContainsKey(actorNumber) || !playerReadyStatus[actorNumber])
                return false;
        }
        return true;
    }

    public static Dictionary<int, bool> GetPlayerReadyStatus()
    {
        // ���� ��ȯ�Ͽ� ������ ���� ������ ��
        return new Dictionary<int, bool>(playerReadyStatus);
    }

    [PunRPC]
    public void ResetAllReadyStates()
    {
        ResetReadyState();
        
        foreach(var plyaer in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = plyaer.Value.ActorNumber;
            PV.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, false);
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
                Debug.LogError($"[RPC_RegisterCharacter] CharacterBase ������Ʈ�� ã�� �� ����, ViewID: {viewID}");
            }
        }
        else
        {
            Debug.LogError($"[RPC_RegisterCharacter] PhotonView�� ã�� �� ����, ViewID: {viewID}");
        }
    }
    
    [PunRPC]
    public void RPC_HandleDeath(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            CharacterBase characterBase = pv.GetComponent<CharacterBase>();
            if (characterBase != null)
            {
                StageLifeManager.Instance.OnCharacterDeath(characterBase);
            }
            else
            {
                Debug.LogError($"[RPC_HandleDeath] CharacterBase ������Ʈ�� ã�� �� ����, ViewID: {viewID}");
            }
        }
        else
        {
            Debug.LogError($"[RPC_HandleDeath] PhotonView�� ã�� �� ����, ViewID: {viewID}");
        }
    }

    [PunRPC]
    public void RPC_Revive(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;

        CharacterBase character = view.GetComponent<CharacterBase>();
        if (character == null) return;

        StageLifeManager.Instance.TryRevive();
    }
}
