using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PN = Photon.Pun.PhotonNetwork;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string firstSceneName = "TitleMatch";
    private const string BestRegionKey = "PUNCloudBestRegion";
    private bool isExiting = false;

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
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name != firstSceneName) return;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            AppSettings appSettings = GetAppSettingsFromEnv();

            if (appSettings != null)
            {
                if (PlayerPrefs.HasKey(BestRegionKey))
                {
                    PlayerPrefs.DeleteKey(BestRegionKey);
                    PlayerPrefs.Save();
                }

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

    public override void OnJoinedRoom()
    {
        AppSettings appSettings = GetAppSettingsFromEnv();

        if (VoiceManager.Instance != null && VoiceManager.Instance.voiceClient != null)
        {
            if (!VoiceManager.Instance.voiceClient.Client.IsConnected)
                VoiceManager.Instance.ConnectVoice(appSettings);
        }
    }

    public bool IsInRoomAndReady()
    {
        return Instance && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom;
    }

    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene().name == "TitleMatch") return;
        if (isExiting) return;

        isExiting = true;

        ResetGameAndLoadTitle();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().name == "TitleMatch") return;
        if (isExiting) return;

        isExiting = true;

        ResetGameAndLoadTitle();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().name == "TitleMatch") return;
        if (isExiting) return;

        isExiting = true;

        ResetGameAndLoadTitle();
        Debug.Log("Disconnected");
    }

    public AppSettings GetAppSettingsFromEnv()
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
            AppIdVoice = voiceAppId,
            FixedRegion = "asia",
            AppVersion = "1.0"
        };

        return appSettings;
    }

    /// <summary>
    /// 보이스 연결을 끊고 중복되어 충돌할 수 있는 매니저들 정리
    /// </summary>
    [PunRPC]
    private void ReturnToTitle()
    {
        if (UIManager.Instance)
            UIManager.Instance.CloseAll();

        if (VoiceManager.Instance && VoiceManager.Instance.voiceClient.Client.IsConnected)
        {
            VoiceManager.Instance.voiceClient.Client.Disconnect();
        }

        SceneManager.LoadScene(firstSceneName);
        CleanUpDuplicateManagers();
    }

    void ResetGameAndLoadTitle()
    {
        if (SceneManager.GetActiveScene().name != firstSceneName)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(ReturnToTitle), RpcTarget.All);
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                ReturnToTitle();
            }
        }
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

        RPCManager[] rpcManagers = FindObjectsOfType<RPCManager>(true);
        foreach (var rpcManager in rpcManagers)
        {
            if (rpcManager != RPCManager.Instance)
                Destroy(rpcManager.gameObject);
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
        if (RPCManager.Instance)
            Destroy(RPCManager.Instance.gameObject);
        if (NetworkManager.Instance)
            Destroy(NetworkManager.Instance.gameObject);
    }

    /// <summary>
    /// 네트워크 정리 완료 후 게임 종료
    /// </summary>
    public void QuitGameSafely()
    {
        StartCoroutine(QuitGameCoroutine());
    }

    /// <summary>
    /// 안전한 게임 종료
    /// </summary>
    private IEnumerator QuitGameCoroutine()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        Application.Quit();
    }
}
