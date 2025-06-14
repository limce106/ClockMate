using Define;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    public static LoadingManager Instance { get; private set; }

    private UILoading _uiLoading;
    private bool _isLoading = false;
    private AsyncOperation currentLoadOperation;

    private Dictionary<int, float> loadingProgress = new Dictionary<int, float>();
    private HashSet<int> loadedPlayers = new HashSet<int>();

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
        StartCoroutine("EndLoading");
    }

    public void StartSyncedLoading(string nextSceneName)
    {
        if(_isLoading) 
            return;

        _isLoading = true;
        _uiLoading = UIManager.Instance.Show<UILoading>("UILoading");

        if (nextSceneName == null)
        {
            Debug.Log("Next Scene Name Is Null!");
            return;
        }

        StartCoroutine(LoadSceneAsync(nextSceneName));
    }

    private IEnumerator LoadSceneAsync(string nextSceneName)
    {
        currentLoadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        currentLoadOperation.allowSceneActivation = false;

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        while(!currentLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            _uiLoading.UpdateLoadingProgress(progress);
            
            if(progress >= 1f)
            {
                photonView.RPC("NotifyPlayerLoaded", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                break;
            }

            yield return null;
        }
    }

    [PunRPC]
    void NotifyPlayerLoaded(int actorNumber)
    {
        if(!loadedPlayers.Contains(actorNumber))
        {
            loadedPlayers.Add(actorNumber);
        }

        if (loadedPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("ActivateLoadedScene", RpcTarget.All);
        }
    }

    [PunRPC]
    void ActivateLoadedScene()
    {
        if(currentLoadOperation != null)
        {
            currentLoadOperation.allowSceneActivation = true;
        }
    }

    IEnumerator EndLoading()
    {
        if (_isLoading)
        {
            yield return new WaitUntil(() => GameManager.Instance.Characters?.Count >= 2);
            GameManager.Instance?.ResetStageAndCharacter();
        }

        yield return new WaitForSeconds(3f);

        _isLoading = false;

        if (_uiLoading != null)
        {
            _uiLoading.Close();
            _uiLoading = null;
        }
    }
}
