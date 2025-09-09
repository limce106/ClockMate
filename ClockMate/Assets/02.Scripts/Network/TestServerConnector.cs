using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Define;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using ExitGames.Client.Photon;

public class TestServerConnector : MonoBehaviourPunCallbacks
{
    private readonly string RoomName = "ClockMate_TestServer";

    public GameObject enterTestServerButton;
    public TMP_Text statusText;
    public GameObject puzzleHUD;

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
            PhotonNetwork.AutomaticallySyncScene = false;

            AppSettings appSettings = GetAppSettingsFromEnv();
            if (appSettings != null)
            {
                PhotonNetwork.ConnectUsingSettings(appSettings);
            }
        }
        else
        {
            statusText.text = "이미 서버 연결됨!";
        }

        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
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

    public override void OnJoinedLobby()
    {
        AppSettings appSettings = GetAppSettingsFromEnv();
        if (VoiceManager.Instance != null && appSettings != null)
        {
            VoiceManager.Instance.ConnectVoice(appSettings);
        }
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
            var hour = PhotonNetwork.Instantiate("Characters/Hour", hourSpawnPos, Quaternion.identity);
            GameManager.Instance.RegisterCharacter(Character.CharacterName.Hour, hour.GetComponent<CharacterBase>());
            GameManager.Instance?.SetSelectedCharacter(Character.CharacterName.Hour);
        }
        else
        {
            var milli = PhotonNetwork.Instantiate("Characters/Milli", milliSpawnPos, Quaternion.identity);
            GameManager.Instance.RegisterCharacter(Character.CharacterName.Milli, milli.GetComponent<CharacterBase>());
            GameManager.Instance?.SetSelectedCharacter(Character.CharacterName.Milli);
        }

        PuzzleHUD puzzleHUD = UIManager.Instance?.Show<PuzzleHUD>("PuzzleHUD");

        CinemachineTargetSetter cinemachineTargetSetter = FindObjectOfType<CinemachineTargetSetter>();
        if (cinemachineTargetSetter != null)
            cinemachineTargetSetter.SetTarget();
    }

    private AppSettings GetAppSettingsFromEnv()
    {
        EnvLoader.LoadEnv();

        string punAppId = EnvLoader.GetEnv("PUN_APP_ID");
        string voiceAppId = EnvLoader.GetEnv("VOICE_APP_ID");

        if (string.IsNullOrEmpty(punAppId))
        {
            return null;
        }

        AppSettings appSettings = new AppSettings
        {
            AppIdRealtime = punAppId,
            AppIdVoice = voiceAppId
        };

        return appSettings;
    }
}
