using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [Header("Text")]
    public TMP_InputField joinCodeInputField;
    public TMP_Text joinCodeText;
    public TMP_Text statusText;

    [Header("Panel")]
    public GameObject lobbyPanel;
    public GameObject playTypePanel;
    public GameObject connectPanel;
    public GameObject player1Panel;
    public GameObject player2Panel;

    private string joinCode;
    private const int MaxPlayer = 2;
    private const int MaxRetry = 3;
    private const int RoomCodeLen = 6;

    private static readonly char[] RoomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    // 친구와 함께하기
    public void OnClick_CreateRoom()
    {
        StartCoroutine(TryCreateRoom());
    }

    IEnumerator TryCreateRoom()
    {
        int retry = MaxRetry;

        while (retry > 0)
        {
            joinCode = GenerateRoomCode();
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = MaxPlayer,
                IsVisible = false,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(joinCode, options, TypedLobby.Default);
            statusText.text = "방 생성 중...";

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
        statusText.text = "방에 입장 중...";
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

    // 랜덤 매치(현재는 사용 안 함)
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
        ShowConnectUI();

        if(PhotonNetwork.IsMasterClient && RPCManager.Instance == null)
        {
            PhotonNetwork.Instantiate("Prefabs/RPCManager", Vector3.zero, Quaternion.identity);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("SetPlayer2PanelActive", RpcTarget.All, true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("UpdateStatusText", RpcTarget.All, "E키를 눌러 게임을 시작하세요.");

        StartCoroutine(WaitAndSetupRPC());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        photonView.RPC("SetPlayer2PanelActive", RpcTarget.All, false);
        photonView.RPC("UpdateStatusText", RpcTarget.All, "상대가 나갔습니다. 상대를 기다리는 중...");

        RPCManager.Instance.photonView.RPC("SetCanAcceptReady", RpcTarget.All, false);
    }

    [PunRPC]
    void SetPlayer2PanelActive(bool isActive)
    {
        player2Panel.SetActive(isActive);
    }

    [PunRPC]
    void UpdateStatusText(string message)
    {
        statusText.text = message;
    }

    private void ShowConnectUI()
    {
        lobbyPanel.SetActive(false);
        playTypePanel.SetActive(false);
        connectPanel.SetActive(true);

        if (joinCode == null)
        {
            joinCode = PhotonNetwork.CurrentRoom.Name;
        }
        joinCodeText.text = joinCode;
        statusText.text = "상대를 기다리는 중...";
    }

    IEnumerator WaitAndSetupRPC()
    {
        float timeout = 3f;
        float timer = 0f;

        while((RPCManager.Instance == null || RPCManager.Instance.photonView == null) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return null;

        if (RPCManager.Instance != null)
        {
            RPCManager.Instance.photonView.RPC("SetCanAcceptReady", RpcTarget.All, true);
            RPCManager.OnSyncedAllReadyAction = () =>
            {
                PhotonNetwork.LoadLevel("CharacterSelect");
            };
        }
        else
        {
            Debug.Log("RPCManager 초기화 실패!");
        }
    }
}