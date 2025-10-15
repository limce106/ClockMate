using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.SceneManagement.SceneManager;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class PlayerConnectSlot
    {
        public TMP_Text playerName;
        public Image readyImg;
        public int actorNumber = -1;
    }

    [Header("UI")]
    public TMP_InputField joinCodeInputField;
    public TMP_Text joinCodeText;
    public TMP_Text joinCodeStatusText;
    public TMP_Text connectStatusText;
    public Image EKey;

    [Header("Panel")]
    public GameObject titlePanel;
    public GameObject joinCodePanel;
    public GameObject playTypePanel;
    public GameObject connectPanel;

    [SerializeField] private PlayerConnectSlot[] players;

    private string _joinCode;
    private HashSet<int> readyPlayerId = new HashSet<int>();

    private const int MaxPlayer = 2;
    private const int MaxRetry = 3;
    private const int RoomCodeLen = 6;

    private static readonly char[] RoomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            photonView.RPC(nameof(SetReady), RpcTarget.All, FindMySlotNum(), PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    // 친구와 함께하기
    public void OnClick_CreateRoom()
    {
        StartCoroutine(TryCreateRoom());
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    IEnumerator TryCreateRoom()
    {
        int retry = MaxRetry;

        while (retry > 0)
        {
            _joinCode = GenerateRoomCode();
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = MaxPlayer,
                IsVisible = false,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(_joinCode, options, TypedLobby.Default);

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
    }

    public void OnClick_JoinWithCode()
    {
        string code = joinCodeInputField.text.ToUpper();

        if (code.Length != RoomCodeLen)
        {
            joinCodeStatusText.text = "코드는 6자리여야 합니다.";
            return;
        }

        PhotonNetwork.JoinRoom(code);
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
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
        joinCodeStatusText.text = "초대 코드가 잘못 되었거나 방이 꽉 찼어요!";
        Debug.LogWarning($"JoinRoom 실패: {message}");
    }

    // 랜덤 매치(현재는 사용 안 함)
    public void OnClick_RandomMatch()
    {
        PhotonNetwork.JoinRandomRoom();
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
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
    }

    public override void OnJoinedRoom()
    {
        ShowConnectUI();

        int slotNum = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        players[slotNum].actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (PhotonNetwork.IsMasterClient)
        {
            if (RPCManager.Instance == null)
                PhotonNetwork.Instantiate("Prefabs/RPCManager", Vector3.zero, Quaternion.identity);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("RPC_SetPlayer2Name", RpcTarget.All, "플레이어 2");
        photonView.RPC("SetEKeyActive", RpcTarget.All, true);
        photonView.RPC("UpdateStatusText", RpcTarget.All, "E키를 눌러 게임을 시작하세요.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RPC_SetPlayer2Name("...");
        SetEKeyActive(false);
        UpdateStatusText("함께 모험 할 동료를 기다리는 중..");
    }

    [PunRPC]
    private void SetEKeyActive(bool isActive)
    {
        EKey.gameObject.SetActive(isActive);
    }

    [PunRPC]
    void RPC_SetPlayer2Name(string name)
    {
        players[1].playerName.text = name;
    }

    [PunRPC]
    void UpdateStatusText(string message)
    {
        connectStatusText.text = message;
    }

    private void ShowConnectUI()
    {
        joinCodePanel.SetActive(false);
        playTypePanel.SetActive(false);
        connectPanel.SetActive(true);

        if (_joinCode == null)
        {
            _joinCode = PhotonNetwork.CurrentRoom.Name;
        }
        joinCodeText.text = _joinCode;
        connectStatusText.text = "함께 모험 할 동료를 기다리는 중..";
    }
    
    [PunRPC]
    void LoadCharacterSelectScene()
    {
        LoadScene("CharacterSelect");
        Debug.Log("CharacterSelect");
    }

    public void OnClick_ConnectBack()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            connectPanel.SetActive(false);
            titlePanel.SetActive(true);
        }

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    private int FindMySlotNum()
    {
        for(int i = 0; i < players.Length; i++)
        {
            var slot = players[i];

            if (slot.actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                return i;
        }

        return -1;
    }

    [PunRPC]
    private void SetReady(int playerSlotNum, int actorNum)
    {
        bool isActive = !players[playerSlotNum].readyImg.gameObject.activeSelf;
        players[playerSlotNum].readyImg.gameObject.SetActive(isActive);

        if (isActive)
            readyPlayerId.Add(actorNum);
        else
            readyPlayerId.Remove(actorNum);

        if (readyPlayerId.Count == 2)
            photonView.RPC(nameof(LoadCharacterSelectScene), RpcTarget.All);
    }

    public override void OnLeftRoom()
    {
        connectPanel.SetActive(false);
        titlePanel.SetActive(true);
    }
}