using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Loading;
using static Define.Map;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    public static LoadingManager Instance { get; private set; }

    private UILoading _uiLoading;
    public bool isLoading { private set; get; } = false;
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        SoundManager.Instance.StopAll(SoundType.BGM);
        SoundManager.Instance.StopAll(SoundType.Effect);

        _uiLoading.ShowRandomTip(GetRandomTip());
    }

    public void StartSyncedLoading(string nextSceneName)
    {
        if(isLoading) 
            return;

        if (nextSceneName == null)
        {
            Debug.Log("Next Scene Name Is Null!");
            return;
        }

        isLoading = true;
        _targetScene = nextSceneName;
        StartCoroutine(LoadSceneAsync(nextSceneName));
    }

    private void StartMoveCharacter(string nextSceneName)
    {
        string currentMap = "";

        if(SceneManager.GetActiveScene().name == "TitleMatch" || SceneManager.GetActiveScene().name == "CharacterSelect")
        {
            currentMap = "Village";
        }
        else
        {
            currentMap = SceneManager.GetActiveScene().name;
        }

        LDLoadingPosition currentLoadingPos = LocalDataManager.Instance.LoadingPosition.DataList.
            Where(data => data.Map.ToString() == currentMap).First<LDLoadingPosition>();

        LDLoadingPosition nextLoadingPos = LocalDataManager.Instance.LoadingPosition.DataList.
            Where(data => data.Map.ToString() == currentMap).First<LDLoadingPosition>();


        Vector2 moveStartPos = new Vector2(currentLoadingPos.PosX, currentLoadingPos.PosY);
        Vector2 moveEndPos = new Vector2(nextLoadingPos.PosX, nextLoadingPos.PosY);

        _uiLoading.StartCoroutine(_uiLoading.MoveCharacater(moveStartPos, moveEndPos));
    }

    private IEnumerator LoadSceneAsync(string nextSceneName)
    {
        StartMoveCharacter(nextSceneName);

        _currentLoadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        _currentLoadOperation.allowSceneActivation = false;

        while (!_currentLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(_currentLoadOperation.progress / 0.9f);
            
            if(progress >= 1f)
            {
                break;
            }

            yield return null;
        }
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
        yield return new WaitUntil(() 
                => GameManager.Instance.Characters?.Count >= PhotonNetwork.CurrentRoom.PlayerCount && 
                   GameManager.Instance.Characters.Values.All(c => c != null)
        );

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.SetAllCharactersActive(false);
        }
        
        yield return new WaitForSeconds(1f);

        if (_uiLoading != null)
        {
            _uiLoading.Close();
            _uiLoading = null;
        }

        isLoading = false;
        _targetScene = null;

        string currentScene = SceneManager.GetActiveScene().name;

        if (PhotonNetwork.IsMasterClient)
        {
            if (GameManager.Instance.CurrentStage.Map.HasCinematicIntro())
            {
                CutsceneSyncManager.Instance.PlayCinematicForAll(
                    cutsceneName: currentScene + "_Intro",
                    masterOnlyOnAllFinished:
                    () =>
                    {
                        photonView.RPC(nameof(RPC_FinishIntroAndActivatePlayerControl), RpcTarget.All, currentScene);
                    });
            }
            else
            {
                photonView.RPC(nameof(RPC_FinishIntroAndActivatePlayerControl), RpcTarget.All, currentScene);
            }
        }
        GameManager.Instance.PlayMapBgm();
    }
    
    [PunRPC]
    private void RPC_FinishIntroAndActivatePlayerControl(string currentScene)
    {
        CharacterBase character = GameManager.Instance.Characters[GameManager.Instance.SelectedCharacter];
        character.photonView.RPC(nameof(character.SetCharacterActive), RpcTarget.All, true);
        GameManager.Instance.SetLocalCharacterInput(true);

        if (currentScene == "ClockTower")
        {
            PuzzleHUD puzzleHUD = UIManager.Instance.Show<PuzzleHUD>("PuzzleHUD");
            puzzleHUD.HideQuest();
        }
        else
        {
            StartCoroutine(HandleMapDescriptionAndTutorial(currentScene));
        }
    }

    private IEnumerator HandleMapDescriptionAndTutorial(string currentScene)
    {
        if (currentScene == "Desert" && GameManager.Instance.CurrentStage.ID == 1)
        {
            // 카메라 전환 기다리는 시간
            yield return new WaitForSeconds(2f);

            UITutorial tutorialUI = UIManager.Instance.Show<UITutorial>("UITutorial", true);
            yield return new WaitUntil(() => !UIManager.Instance.IsOnScreen(tutorialUI));

            PuzzleHUD puzzleHUD = UIManager.Instance.Show<PuzzleHUD>("PuzzleHUD");
            puzzleHUD.ShowMapDescription();
            puzzleHUD.ShowAndUpdateQuest();
        }
        else
        {
            PuzzleHUD puzzleHUD = UIManager.Instance.Show<PuzzleHUD>("PuzzleHUD");
            puzzleHUD.ShowMapDescription();
            puzzleHUD.ShowAndUpdateQuest();
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
