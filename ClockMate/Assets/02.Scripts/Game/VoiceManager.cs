using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PN = Photon.Pun.PhotonNetwork;

public class VoiceManager : MonoBehaviour, IConnectionCallbacks, IInRoomCallbacks
{
    public static VoiceManager Instance;

    public Recorder recorder;
    public PunVoiceClient voiceClient;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        voiceClient = GetComponent<PunVoiceClient>();
        recorder = GetComponent<Recorder>();
    }

    private void OnDestroy()
    {
        if (voiceClient != null && voiceClient.Client != null)
        {
            voiceClient.Client.RemoveCallbackTarget(this);
        }
    }

    private void Start()
    {
        //foreach (var device in Microphone.devices)
        //{
        //    Debug.Log($"감지된 마이크 장치: {device}");
        //}
    }

    public void SetMicActive(bool isActive)
    {
        recorder.TransmitEnabled = isActive;
    }

    /// <summary>
    /// PUN에서 룸 접속 성공 후 호출되어 Voice 서버 연결 시도
    /// </summary>
    public void ConnectVoice(AppSettings appSettings)
    {
        if (voiceClient != null && voiceClient.Client != null)
        {
            voiceClient.Client.AddCallbackTarget(this);
            voiceClient.Client.AppVersion = "1.0";
        }
        else
        {
            Debug.LogError("Voice Client 객체 초기화 실패. 콜백을 등록할 수 없습니다.");
            return;
        }

        voiceClient.Client.AppId = appSettings.AppIdVoice;
        voiceClient.Client.ConnectUsingSettings(appSettings);
    }

    public void OnConnected() { }

    public void OnConnectedToMaster()
    {
        StartCoroutine(WaitForVoiceReadyAndJoinRoom());
    }

    /// <summary>
    /// Voice 클라이언트 상태가 OpJoinOrCreateRoom을 호출할 수 있을 때까지 대기
    /// </summary>
    IEnumerator WaitForVoiceReadyAndJoinRoom()
    {
        if (!PN.InRoom)
        {
            yield break;
        }

        while (voiceClient.Client.State != ClientState.ConnectedToMasterServer && voiceClient.Client.State != ClientState.Joined)
        {
            yield return null;
        }

        if (voiceClient.Client.State == ClientState.Joined)
        {
            yield break;
        }

        // 안정적인 상태(ConnectedToMaster)가 되면 룸 참가 로직 호출
        TryJoinVoiceRoom();
    }

    /// <summary>
    /// 보이스 룸 참가 시도
    /// </summary>
    private void TryJoinVoiceRoom()
    {
        if (voiceClient.Client.State == ClientState.ConnectedToMasterServer && PN.InRoom)
        {
            Debug.Log($"Voice 상태({voiceClient.Client.State})와 PUN 상태 확인 완료. OpJoinOrCreateRoom 호출");
            voiceClient.Client.OpJoinOrCreateRoom(new EnterRoomParams()
            {
                RoomName = PN.CurrentRoom.Name,
                RoomOptions = new RoomOptions() { IsVisible = false, MaxPlayers = (byte)PN.CurrentRoom.MaxPlayers }
            });
        }
        else
        {
            Debug.LogWarning($"Voice 룸 참가 시도 실패: Voice 상태: {voiceClient.Client.State}, PUN InRoom: {PN.InRoom}");
        }
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Voice 서버 연결 끊김. 원인: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }
}
