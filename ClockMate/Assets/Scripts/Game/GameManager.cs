using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Character;

public class GameManager : MonoSingleton<GameManager>
{
    public CharacterName SelectedCharacter { get; private set; }
    public Dictionary<CharacterName, CharacterBase> Characters { get; private set; }
    public BoStage CurrentStage { get; private set; }

    protected override void Init()
    {
        Characters = new Dictionary<CharacterName, CharacterBase>();

        if (SaveManager.Instance.SaveDataExist())
        {
            // 저장 데이터가 존재하면 불러오기
            SaveData saveData = SaveManager.Instance.Load();
            CurrentStage = new BoStage(saveData.stageId);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Desert" && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.IsLocal)
        {
            // 캐릭터 로딩
            LoadCharacter(SelectedCharacter);
        }
    }

    public void CreateNewSaveData()
    {
        // 새 게임 시작 시 호출 필요
        if (!SaveManager.Instance.SaveDataExist())
        {
            // 저장된 데이터가 없으면 (새 게임이면)
            SaveManager.Instance.Save(1); // 사막 맵 stage 1으로 저장
            CurrentStage = new BoStage(1);
        }
    }

    public void StageComplete()
    {
        BoStage nextStage = CurrentStage.NextStage;
        if (nextStage != null)
        {
            // 다음 스테이지 존재하는 경우
            
            SaveManager.Instance.Save(nextStage.ID); // 진행 상태 저장
            if (nextStage.Map != CurrentStage.Map)
            {
                // 이번 맵의 마지막 스테이지일 경우
                ResetTestManager.Instance.RemoveAllResettable();
                LoadingManager.Instance?.StartSyncedLoading(nextStage.Map.ToString());
            }
            CurrentStage = nextStage;
        }
        else
        {
            // 엔딩 처리
        }
    }

    public void ResetStageAndCharacter()
    {
        CurrentStage?.Reset();
        SetAllCharactersActive(true);
    }

    private CharacterBase LoadCharacter(CharacterName characterName)
    {
        if (Characters.ContainsKey(characterName))
            return Characters[characterName];

        string path = $"Characters/{characterName}";

        if(NetworkManager.Instance.IsInRoomAndReady())
        {
            GameObject player = PhotonNetwork.Instantiate(path, Vector3.zero, Quaternion.identity, 0, new object[] { characterName });
            CharacterBase character = player.GetComponent<CharacterBase>();
            character.name = characterName.ToString();

            int viewID = player.GetComponent<PhotonView>().ViewID;
            RPCManager.Instance.photonView.RPC("RPC_RegisterCharacter", RpcTarget.All, characterName, viewID);

            return character;
        }
        else
        {
            Debug.LogError($"[GameManager] 캐릭터 프리팹 로드 실패: 네트워크 연결 불가");
            return null;
        }
    }

    public void SetSelectedCharacter(CharacterName character)
    {
        SelectedCharacter = character;
    }

    public void RegisterCharacter(CharacterName character, CharacterBase characterBase)
    {
        if (!Characters.ContainsKey(character))
        {
            Characters.Add(character, characterBase);
        }
    }

    public void SetAllCharactersActive(bool isActive)
    {
        foreach (CharacterBase character in Characters.Values)
        {
            character.photonView.RPC("SetCharacterActive", RpcTarget.All, isActive);
        }
    }
}