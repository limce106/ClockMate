using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PN = Photon.Pun.PhotonNetwork;
using UnityEngine.SceneManagement;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string firstSceneName = "TitleMatch";

    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get
        {
            if(_instance == null)
            {
                var obj = FindObjectOfType<NetworkManager>();
                if(obj != null)
                    _instance = obj;
            }
            return _instance;
        }
    }

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        PhotonNetwork.AutomaticallySyncScene = false;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            AppSettings appSettings = GetAppSettingsFromEnv();
            if(appSettings != null)
            {
                PhotonNetwork.ConnectUsingSettings(appSettings);
            }
            else
            {
                Debug.LogError("App ID를 불러올 수 없습니다. 연결을 시도하지 않습니다.");
            }
        }


        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master");
    }

    public override void OnJoinedLobby()
    {
        AppSettings appSettings = GetAppSettingsFromEnv();
        if (VoiceManager.Instance != null && appSettings != null)
        {
            VoiceManager.Instance.ConnectVoice(appSettings);
        }
    }

    public bool IsInRoomAndReady()
    {
        return Instance && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        TryHandleDisconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        TryHandleDisconnect();
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

    void TryHandleDisconnect()
    {
        if (SceneManager.GetActiveScene().name != firstSceneName)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ForceReturnToTitle", RpcTarget.All);
            }

            Debug.Log("Disconnected");
        }
    }

    [PunRPC]
    void ForceReturnToTitle()
    {
        StartCoroutine(ReturnToTitleAfterDisconnect());
    }

    IEnumerator ReturnToTitleAfterDisconnect()
    {
        PhotonNetwork.Disconnect();

        while(PhotonNetwork.IsConnected)
            yield return null;

        SceneManager.LoadScene(firstSceneName);
        CleanUpDuplicateManagers();
    }

    /// <summary>
    /// 현재 갖고 있는 NetworkManager, LoadingManager와 타이틀 씬에 존재하는 동일 오브젝트가 충돌하여 PhotonView ID 중복 오류 발생 가능 
    /// 따라서 타이틀 씬 이동 전 현재 NetworkManager, LoadingManager 제거
    /// </summary>
    private void CleanUpDuplicateManagers()
    {
        LoadingManager[] loadingManagers = FindObjectsOfType<LoadingManager>(true);
        foreach (var loadingManager in loadingManagers)
        {
            if (loadingManager != LoadingManager.Instance)
                Destroy(loadingManager.gameObject);
        }

        VoiceManager[] voiceManagers = FindObjectsOfType<VoiceManager>(true);
        foreach (var voiceManager in voiceManagers)
        {
            if (voiceManager != VoiceManager.Instance)
                Destroy(voiceManager.gameObject);
        }

        SoundManager[] soundManagers = FindObjectsOfType<SoundManager>(true);
        foreach (var soundManager in soundManagers)
        {
            if (soundManager != SoundManager.Instance)
                Destroy(soundManager.gameObject);
        }

        CutsceneSyncManager[] cutsceneSyncManagers = FindObjectsOfType<CutsceneSyncManager>(true);
        foreach (var cutsceneSyncManager in cutsceneSyncManagers)
        {
            if (cutsceneSyncManager != CutsceneSyncManager.Instance)
                Destroy(cutsceneSyncManager.gameObject);
        }

        NetworkManager[] networkManagers = FindObjectsOfType<NetworkManager>(true);
        foreach (var networkManager in networkManagers)
        {
            if (networkManager != NetworkManager.Instance)
                Destroy(networkManager.gameObject);
        }

        if (LoadingManager.Instance)
            Destroy(LoadingManager.Instance.gameObject);
        if (VoiceManager.Instance)
            Destroy(VoiceManager.Instance.gameObject);
        if (SoundManager.Instance)
            Destroy(SoundManager.Instance.gameObject);
        if (CutsceneSyncManager.Instance)
            Destroy(CutsceneSyncManager.Instance.gameObject);
        if (NetworkManager.Instance)
            Destroy(NetworkManager.Instance.gameObject);
    }
}
