using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using System;
using DefineExtension;
using static Define.Character;

public class RPCManager : MonoBehaviourPun
{
    private static RPCManager _instance;
    public static RPCManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = FindObjectOfType<RPCManager>();
                if (obj != null)
                    _instance = obj;
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void DeleteAllSaveData()
    {
        SaveManager.Instance?.DeleteSaveData();
    }
    
    [PunRPC]
    public void RPC_HandleDeath(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            CharacterBase characterBase = pv.GetComponent<CharacterBase>();
            if (characterBase != null)
            {
                StageLifeManager.Instance.OnCharacterDeath(characterBase);
            }
            else
            {
                Debug.LogError($"[RPC_HandleDeath] CharacterBase 컴포넌트를 찾을 수 없음, ViewID: {viewID}");
            }
        }
        else
        {
            Debug.LogError($"[RPC_HandleDeath] PhotonView를 찾을 수 없음, ViewID: {viewID}");
        }
    }

    [PunRPC] 
    public void RPC_Revive(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;

        CharacterBase character = view.GetComponent<CharacterBase>();
        if (character == null) return;

        StageLifeManager.Instance.TryRevive();
    }

    [PunRPC]
    public void RPC_SetObjectActive(int viewID, bool active)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if(targetView != null)
        {
            targetView.gameObject.SetActive(active);
        }
    }
    
    [PunRPC]
    public void RPC_MoveToMap(string targetMap)
    {
        ResetTestManager.Instance.RemoveAllResettable();
        LoadingManager.Instance.ShowLoadingUI();
        LoadingManager.Instance.LoadScene(targetMap);
    }

    [PunRPC]
    public void RPC_MoveToStage(int stageID)
    {
        GameManager.Instance.CurrentStage?.Exit();
        BoStage targetStage = new BoStage(stageID);
        GameManager.Instance.SetCurrentStage(stageID);

        if (targetStage.Map.GetMapSceneName() != SceneManager.GetActiveScene().name)
        {
            // 이동하려고 하는 스테이지와 현재 씬이 일치하지 않으면 씬 이동
            ResetTestManager.Instance.RemoveAllResettable();
            LoadingManager.Instance.ShowLoadingUI();
            LoadingManager.Instance.LoadScene(targetStage.Map.GetMapSceneName());
        }
    }
    
    [PunRPC]
    public void RPC_SyncReset()
    {
        GameManager.Instance.CurrentStage?.Reset();
        GameManager.Instance.SetAllCharactersActive(true);
    }
    
    [PunRPC]
    public void RPC_SetSelectedCharacter(int characterNum)
    {
        GameManager.Instance.SetSelectedCharacter((CharacterName) characterNum);
    }
}
