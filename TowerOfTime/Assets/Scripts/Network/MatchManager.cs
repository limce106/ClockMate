using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField joinCodeInputField;
    public TMP_Text statusText;

    private const int MaxPlayer = 2;
    private const int MaxRetry = 3;
    private const int RoomCodeLen = 6;

    private static readonly char[] RoomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    void Awake()
    {
        // 자동 씬 동기화
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "서버에 연결 중...";
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        statusText.text = "서버 연결 완료";
        Debug.Log("Connected to Master");
    }

    // 1. 친구와 함께하기
    public void OnClick_CreateRoom()
    {
        StartCoroutine(TryCreateRoom());
    }

    IEnumerator TryCreateRoom()
    {
        int retry = MaxRetry;

        while (retry > 0)
        {
            string code = GenerateRoomCode();
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = MaxPlayer,
                IsVisible = false,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(code, options, TypedLobby.Default);
            statusText.text = $"방 생성 중... 코드 {code}";

            // Photon 응답 지연 시간
            float elapsed = 0f;
            const float timeout = 2f; // 테스트 후 조정 가능

            while (!PhotonNetwork.InRoom && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (PhotonNetwork.InRoom)
            {
                yield break;
            }

            retry--;
        }

        statusText.text = "방 생성 실패. 다시 시도해주세요.";
    }

    public void OnClick_JoinWithCode()
    {
        string code = joinCodeInputField.text.ToUpper();

        if (code.Length != RoomCodeLen)
        {
            statusText.text = "코드는 6자리여야 합니다.";
            return;
        }

        PhotonNetwork.JoinRoom(code);
        statusText.text = $"{code} 방에 입장 중...";
    }

    private string GenerateRoomCode()
    {
        System.Text.StringBuilder code = new System.Text.StringBuilder();

        for (int i = 0; i < RoomCodeLen; i++)
        {
            int index = Random.Range(0, RoomCodeChars.Length);
            code.Append(RoomCodeChars[index]);
        }

        return code.ToString();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "초대 코드가 잘못 되었거나 방이 꽉 찼어요!";
        Debug.LogWarning($"JoinRoom 실패: {message}");
    }

    // 2.랜덤 매치
    public void OnClick_RandomMatch()
    {
        PhotonNetwork.JoinRandomRoom();
        statusText.text = "랜덤 매칭 중...";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MaxPlayer,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(null, options);
        statusText.text = "새 방 생성 중...";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"방 입장. 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}/{MaxPlayer}";

        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayer)
        {
            Debug.Log("모든 플레이어 입장 완료. 게임 시작");
            // 두 플레이어가 사막 씬으로 이동
            PhotonNetwork.LoadLevel("Desert");
        }
    }

    // 연결 실패 시
    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = "서버 연결 끊김: " + cause.ToString();
    }
}
