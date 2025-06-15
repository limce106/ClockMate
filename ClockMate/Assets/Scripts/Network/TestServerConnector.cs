using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TestServerConnector : MonoBehaviourPunCallbacks
{
    public TMP_Text statusText;

    private readonly string roomName = "ClockMate_TestServer";
    public bool isSpawnPlayer = false;
    public Vector3 milliSpawnPos = new Vector3 (-4.22f, 0.7f, 63f);
    public Vector3 hourSpawnPos = new Vector3 (-9.22f, 0.7f, 63f);

    private void Start()
    {
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = "클릭해서 서버 연결하기";
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            statusText.text = "이미 서버 연결됨!";
        }
    }

    public void EnterTestServerRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = false,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (message.Contains("No match found") || returnCode == ErrorCode.GameDoesNotExist)
        {
            CreateRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        this.gameObject.SetActive(false);

        if (!isSpawnPlayer)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Characters/Hour", hourSpawnPos, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate("Characters/Milli", milliSpawnPos, Quaternion.identity);
        }
    }
}
