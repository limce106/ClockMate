using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.SceneManagement.SceneManager;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchManager : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class PlayerConnectSlot
    {
        public TMP_Text playerName;
        public Image readyImg;
        [HideInInspector] public int actorNumber = -1; // actorNumber는 로컬에서만 사용됨. 동기화는 Room Custom Properties를 통해 이루어짐
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
    private HashSet<int> readyPlayerId = new HashSet<int>(); // 준비 상태

    private const int MaxPlayer = 2;
    private const int MaxRetry = 3;
    private const int RoomCodeLen = 6;

    private const string SLOT_KEY_PREFIX = "Slot_"; // 슬롯 상태를 방 속성에 저장하기 위한 키
    private const int Player1Slot = 0; // 슬롯 인덱스 0
    private const int Player2Slot = 1; // 슬롯 인덱스 1

    private static readonly char[] RoomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            int slotNum = FindMySlotNum();
            if (slotNum != -1)
            {
                photonView.RPC(nameof(SetReady), RpcTarget.All, slotNum, PhotonNetwork.LocalPlayer.ActorNumber);
            }
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
            const float timeout = 2f;

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
        UpdatePlayerSlotsUI();

        if (PhotonNetwork.IsMasterClient)
        {
            //if (RPCManager.Instance == null)
            //    PhotonNetwork.Instantiate("Prefabs/RPCManager", Vector3.zero, Quaternion.identity);

            AssignSlotToNewPlayer(PhotonNetwork.LocalPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 다른 플레이어가 방에 들어왔을 때 마스터 클라이언트가 슬롯 할당
        if (PhotonNetwork.IsMasterClient)
        {
            AssignSlotToNewPlayer(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 준비 완료 목록에서 나간 플레이어 제거
        readyPlayerId.Remove(otherPlayer.ActorNumber);

        // 플레이어가 나갔을 때 마스터 클라이언트가 슬롯 해제
        if (PhotonNetwork.IsMasterClient)
        {
            ClearSlotForPlayer(otherPlayer);
        }
    }

    /// <summary>
    /// 방 속성 변경 시 모든 클라이언트에서 호출되어 UI를 업데이트
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        UpdatePlayerSlotsUI();
    }

    /// <summary>
    /// 새로운 플레이어에게 가장 앞쪽의 빈 슬롯을 할당
    /// </summary>
    private void AssignSlotToNewPlayer(Player playerToAssign)
    {
        // 비어있는 슬롯 인덱스를 찾는다.
        int assignedSlot = -1;
        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        if (!roomProps.ContainsKey(SLOT_KEY_PREFIX + Player1Slot))
        {
            assignedSlot = Player1Slot;
        }
        else if (!roomProps.ContainsKey(SLOT_KEY_PREFIX + Player2Slot))
        {
            assignedSlot = Player2Slot;
        }

        if (assignedSlot != -1)
        {
            // 방 속성에 슬롯 정보를 업데이트
            Hashtable props = new Hashtable();
            props[SLOT_KEY_PREFIX + assignedSlot] = playerToAssign.ActorNumber;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            Debug.LogError($"슬롯 할당 실패: {playerToAssign.NickName}. 방이 꽉 찼습니다.");
        }
    }

    /// <summary>
    /// 나간 플레이어가 차지했던 슬롯을 비움
    /// </summary>
    private void ClearSlotForPlayer(Player playerToClear)
    {
        Hashtable props = new Hashtable();
        bool found = false;

        // 나간 플레이어의 ActorNumber를 방 속성에서 찾는다.
        foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            string key = entry.Key.ToString();
            if (key.StartsWith(SLOT_KEY_PREFIX) && (int)entry.Value == playerToClear.ActorNumber)
            {
                props.Add(key, null); // 해당 슬롯 키를 방 속성에서 제거
                found = true;
                break;
            }
        }

        if (found)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(props); // 변경된 속성 전송
        }
    }


    /// <summary>
    /// 모든 클라이언트에 대해 슬롯 UI 업데이트
    /// </summary>
    private void UpdatePlayerSlotsUI()
    {
        // 모든 슬롯을 기본값으로 초기화
        for (int i = 0; i < players.Length; i++)
        {
            players[i].playerName.text = (i == Player1Slot) ? "..." : "...";
            players[i].actorNumber = -1;
            players[i].readyImg.gameObject.SetActive(false);
        }

        // 방 속성에 따라 슬롯을 채운다.
        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach (DictionaryEntry entry in roomProps)
        {
            string key = entry.Key.ToString();

            if (key.StartsWith(SLOT_KEY_PREFIX))
            {
                // 키에서 슬롯 인덱스를 추출합니다. (예: "Slot_0" -> 0)
                if (int.TryParse(key.Substring(SLOT_KEY_PREFIX.Length), out int slotIndex))
                {
                    int actorNumber = (int)entry.Value;

                    if (slotIndex >= 0 && slotIndex < players.Length)
                    {
                        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

                        if (player != null)
                        {
                            players[slotIndex].actorNumber = actorNumber;
                            players[slotIndex].playerName.text = (slotIndex == Player1Slot) ? "플레이어 1" : "플레이어 2";

                            // 준비 상태 동기화
                            if (readyPlayerId.Contains(actorNumber))
                            {
                                players[slotIndex].readyImg.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        // UI 상태 텍스트 및 E키 활성화 업데이트
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount == MaxPlayer)
        {
            SetEKeyActive(true);
            UpdateStatusText("E키를 눌러 게임을 시작하세요.");
        }
        else
        {
            SetEKeyActive(false);
            UpdateStatusText("함께 모험 할 동료를 기다리는 중..");
        }
    }

    [PunRPC]
    private void SetEKeyActive(bool isActive)
    {
        EKey.gameObject.SetActive(isActive);
    }

    [PunRPC]
    void UpdateStatusText(string message)
    {
        connectStatusText.text = message;
    }

    private void ShowConnectUI()
    {
        joinCodeInputField.text = "";
        joinCodePanel.SetActive(false);
        playTypePanel.SetActive(false);
        connectPanel.SetActive(true);

        if (_joinCode == null)
        {
            _joinCode = PhotonNetwork.CurrentRoom.Name;
        }
        joinCodeText.text = _joinCode;
    }

    [PunRPC]
    void LoadCharacterSelectScene()
    {
        LoadScene("CharacterSelect");
    }

    public void OnClick_ConnectBack()
    {
        if (PhotonNetwork.InRoom)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(MasterClientLeaveRoom());
            }
            else
            {
                PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            connectPanel.SetActive(false);
            titlePanel.SetActive(true);
        }

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    /// <summary>
    /// 현재 로컬 플레이어의 슬롯 번호를 Room Custom Properties를 기반으로 찾는다.
    /// </summary>
    private int FindMySlotNum()
    {
        int myActorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        foreach (DictionaryEntry entry in roomProps)
        {
            string key = entry.Key.ToString();
            if (key.StartsWith(SLOT_KEY_PREFIX))
            {
                if ((int)entry.Value == myActorNum)
                {
                    if (int.TryParse(key.Substring(SLOT_KEY_PREFIX.Length), out int slotIndex))
                    {
                        return slotIndex;
                    }
                }
            }
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

        if (readyPlayerId.Count == MaxPlayer && PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (PlayModeSelector.IsNewGameRoom)
            {
                photonView.RPC(nameof(LoadCharacterSelectScene), RpcTarget.All);
            }
            else
            {
                GameManager.Instance.SetStageWithExistingData();
            }

        }
    }

    public override void OnLeftRoom()
    {
        connectPanel.SetActive(false);
        titlePanel.SetActive(true);
        readyPlayerId.Clear();
    }

    [PunRPC]
    private void ForceLeaveRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    /// <summary>
    /// 마스터 이양 문제 방지를 위해 비마스터 먼저 퇴장
    /// </summary>
    IEnumerator MasterClientLeaveRoom()
    {
        // 비마스터 먼저 퇴장
        photonView.RPC(nameof(ForceLeaveRoom), RpcTarget.Others);

        yield return new WaitForSeconds(0.5f);

        // 마스터도 퇴장
        PhotonNetwork.LeaveRoom();
    }

    public void ResetStatusAndJoinCode()
    {
        joinCodeStatusText.text = "";
        joinCodeInputField.text = "";
    }
}