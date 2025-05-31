using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PN = Photon.Pun.PhotonNetwork;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if(instance == null)
            {
                var obj = FindObjectOfType<NetworkManager>();
                if(obj != null)
                    instance = obj;
            }
            return instance;
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 자동 씬 동기화
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected");
    }

    public void LeaveGame()
    {
        PN.Disconnect();
    }

    public bool IsInRoomAndReady()
    {
        return Instance && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom;
    }
}
