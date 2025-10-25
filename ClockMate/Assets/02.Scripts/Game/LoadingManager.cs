using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Loading;
using static Define.Map;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    public static LoadingManager Instance { get; private set; }

    private UILoading _uiLoading;
    public bool _isLoading { private set; get; } = false;
    private AsyncOperation _currentLoadOperation;
    private string _targetScene;

    private Dictionary<int, float> _loadingProgress = new Dictionary<int, float>();
    private HashSet<int> _finishedPlayers = new HashSet<int>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    new private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    new private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _targetScene)
        {
            photonView.RPC(nameof(NotifyLoadState), RpcTarget.MasterClient, 
                PhotonNetwork.LocalPlayer.ActorNumber, (int) LoadState.Active);
        }
    }

    public void ShowLoadingUI()
    {
        _uiLoading = UIManager.Instance.Show<UILoading>("UILoading");
        GameManager.Instance.SetLocalCharacterInput(false);

        _uiLoading.ShowRandomTip(GetRandomTip());
    }

    public void StartSyncedLoading(string nextSceneName)
    {
        if(_isLoading) 
            return;

        if (nextSceneName == null)
        {
            Debug.Log("Next Scene Name Is Null!");
            return;
        }
        
        _isLoading = true;
        _targetScene = nextSceneName;
        StartCoroutine(LoadSceneAsync(nextSceneName));
    }

    private void StartMoveCharacter(string nextSceneName)
    {
        LDLoadingPosition nextLoadingPos = LocalDataManager.Instance.LoadingPosition.DataList.
            Where(data => data.Map.ToString() == nextSceneName).First<LDLoadingPosition>();

        Vector2 moveEndPos = new Vector2(nextLoadingPos.PosX, nextLoadingPos.PosY);
        _uiLoading.StartCoroutine(_uiLoading.MoveCharacater(moveEndPos));
    }

    private IEnumerator LoadSceneAsync(string nextSceneName)
    {
        _currentLoadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        _currentLoadOperation.allowSceneActivation = false;

        while (!_currentLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(_currentLoadOperation.progress / 0.9f);
            _uiLoading.UpdateLoadingProgress(progress);
            
            if(progress >= 1f)
            {
                break;
            }

            yield return null;
        }

        StartMoveCharacter(nextSceneName);
    }

    [PunRPC]
    void NotifyLoadState(int actorNumber, int loadState)
    {
        _finishedPlayers.Add(actorNumber);
        if (_finishedPlayers.Count != PhotonNetwork.CurrentRoom.PlayerCount) return;

        switch ((LoadState) loadState)
        {
            case LoadState.Load:
                photonView.RPC(nameof(ActivateLoadedScene), RpcTarget.All);
                break;
            case LoadState.Active:
                photonView.RPC(nameof(RPC_InstantiateCharacters), RpcTarget.All);
                break;
        }
        
        _finishedPlayers.Clear();
    }

    [PunRPC]
    private void ActivateLoadedScene()
    {
        if(_currentLoadOperation != null)
        {
            _currentLoadOperation.allowSceneActivation = true;
        }
    }
    
    [PunRPC]
    private void RPC_InstantiateCharacters()
    {
        GameManager.Instance.LoadSelectedCharacter();
        StartCoroutine(nameof(EndLoading));
    }

    IEnumerator EndLoading()
    {
        yield return new WaitUntil(() => GameManager.Instance.Characters?.Count >= 2);

        CinemachineTargetSetter cinemachineTargetSetter = FindObjectOfType<CinemachineTargetSetter>();
        if(cinemachineTargetSetter != null)
            cinemachineTargetSetter.SetTarget();

        yield return new WaitForSeconds(1f);

        if (_uiLoading != null)
        {
            _uiLoading.Close();
            _uiLoading = null;
        }

        _isLoading = false;
        _targetScene = null;

        string currentScene = SceneManager.GetActiveScene().name;

        foreach (PuzzleMapName puzzleMap in Enum.GetValues(typeof(PuzzleMapName)))
        {
            if(currentScene.Equals(puzzleMap.ToString()))
            {
                UIManager.Instance?.Show<PuzzleHUD>("PuzzleHUD");
            }
        }

        GameManager.Instance.SetLocalCharacterInput(true);

        if (currentScene != "ClockTower")
        {
            GameManager.Instance.PlayMapBgm();
            UIManager.Instance.Show<UIMapDescription>("UIMapDescription");
        }
    }

    /// <summary>
    /// 랜덤으로 기후위기 정보를 가져온다.
    /// </summary>
    private string GetRandomTip()
    {
        List<LDClimateTips> tipList = LocalDataManager.Instance.ClimateTips.DataList;

        if (tipList.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, tipList.Count);
            return tipList[randomIndex].Tip;
        }
        else
        {
            Debug.LogWarning("기후 위기 팁 데이터가 없습니다.");
            return "";
        }
    }

    public void LoadScene(string mapName)
    {
        StartSyncedLoading(mapName);
    }
}
