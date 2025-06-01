using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using System;

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
    private bool isLocalPlayerReady = false;
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
            if(isLocalPlayerReady)
            {
                PV.RPC("UnmarkReady", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                isLocalPlayerReady = false;
            }
            else
            {
                PV.RPC("MarkReady", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                isLocalPlayerReady = true;
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

    private void ResetReadyState()
    {
        playerReadyStatus.Clear();
        canAcceptReady = false;
        isLocalPlayerReady = false;
    }

    void TryExecuteOnAllPlayersReady()
    {
        if (!AllPlayersReady())
            return;

        OnLocalAllReadyAction?.Invoke();
        OnLocalAllReadyAction = null;

        PV.RPC("ExecuteSyncedAllPlayersReady", RpcTarget.All);

        ResetReadyState();
    }

    [PunRPC]
    private void ExecuteSyncedAllPlayersReady()
    {
        OnSyncedAllReadyAction?.Invoke();
        OnSyncedAllReadyAction = null;

        ResetReadyState();
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
}
