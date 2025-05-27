using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

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
    private string sceneName = "";

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

    new protected void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    new protected void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerReadyStatus.Clear();
        canAcceptReady = false;
        isLocalPlayerReady = false;
    }

    [PunRPC]
    public void SetCanAcceptReady(bool value)
    {
        canAcceptReady = value;
    }

    [PunRPC]
    public void SetSceneName(string name)
    {
        sceneName = name;
    }

    [PunRPC]
    void MarkReady(int actorNumber)
    {
        PV.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, true);
        TryLoadSceneIfReady();
        Debug.Log("준비 완료");
    }

    [PunRPC]
    void UnmarkReady(int actorNumber)
    {
        PV.RPC("SyncReadyStatus", RpcTarget.All, actorNumber, false);
        Debug.Log("준비 해제");
    }

    // playerReadyStatus는 반드시 SyncReadyStatus를 통해서만 수정할 것
    [PunRPC]
    void SyncReadyStatus(int actorNumber, bool isReady)
    {
        playerReadyStatus[actorNumber] = isReady;
    }

    void TryLoadSceneIfReady()
    {
        if (!PhotonNetwork.IsMasterClient || !AllPlayersReady())
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name not set. Cannot load scene.");
            return;
        }

        PhotonNetwork.LoadLevel(sceneName);
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
        // 복사 반환하여 원본에 영향 없도록 함
        return new Dictionary<int, bool>(playerReadyStatus);
    }
}
