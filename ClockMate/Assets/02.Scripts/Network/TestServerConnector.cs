using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TestServerConnector : MonoBehaviourPunCallbacks
{
    private readonly string RoomName = "ClockMate_TestServer";

    public GameObject enterTestServerButton;
    public TMP_Text statusText;
    public GameObject puzzleHUD;
    public GameObject voiceManager;

    public bool isSpawnPlayer = false;

    public Vector3 hourSpawnPos = new Vector3(0f, 0f, 0f);
    public Vector3 milliSpawnPos = new Vector3(0f, 0f, 0f);

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
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            statusText.text = "아직 네트워크에 연결되지 않았습니다. \n 다시 시도해주세요.";
            return;
        }

        PhotonNetwork.JoinRoom(RoomName);
    }

    void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = false,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(RoomName, options, TypedLobby.Default);
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
        enterTestServerButton.SetActive(false);
        statusText.gameObject.SetActive(false);

        if (!isSpawnPlayer)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Characters/Hour", hourSpawnPos, Quaternion.identity);
            GameManager.Instance?.SetSelectedCharacter(Define.Character.CharacterName.Hour);
        }
        else
        {
            PhotonNetwork.Instantiate("Characters/Milli", milliSpawnPos, Quaternion.identity);
            GameManager.Instance?.SetSelectedCharacter(Define.Character.CharacterName.Milli);
        }

        PuzzleHUD puzzleHUD = UIManager.Instance?.Show<PuzzleHUD>("PuzzleHUD");
    }
}
