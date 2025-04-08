using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Awake()
    {
        // 자동 씬 동기화
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinOrCreateRoom("MainRoom", new RoomOptions {MaxPlayers = 2}, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Lobby");
        // 두 플레이어가 사막 씬으로 이동
        // PhotonNetwork.LoadLevel("Desert");
    }
}
