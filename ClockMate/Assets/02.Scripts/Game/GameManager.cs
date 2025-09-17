using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Character;

public class GameManager : MonoSingleton<GameManager>
{
    [field: SerializeField] public CharacterName SelectedCharacter { get; private set; } = CharacterName.Hour;
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

    public bool LoadSelectedCharacter()
    {
        string path = $"Characters/{SelectedCharacter}";

        if(NetworkManager.Instance.IsInRoomAndReady())
        {
            Vector3 position = CurrentStage.LoadPositions[SelectedCharacter];
            GameObject player = PhotonNetwork.Instantiate(path, position, Quaternion.identity, 0, new object[] { SelectedCharacter });
            CharacterBase character = player.GetComponent<CharacterBase>();
            character.gameObject.name = SelectedCharacter.ToString();
            Debug.Log($"character spawn: {SelectedCharacter}, scene: {SceneManager.GetActiveScene().name}");

            int viewID = player.GetComponent<PhotonView>().ViewID;
            RPCManager.Instance.photonView.RPC("RPC_RegisterCharacter", RpcTarget.All, SelectedCharacter, viewID);

            return true;
        }

        Debug.LogError($"[GameManager] 캐릭터 프리팹({SelectedCharacter}) 로드 실패: 네트워크 연결 불가");
        return false;
    }

    public void SetSelectedCharacter(CharacterName character)
    {
        SelectedCharacter = character;
    }

    public void RegisterCharacter(CharacterName character, CharacterBase characterBase)
    {
        Characters[character] = characterBase;
    }

    public void SetAllCharactersActive(bool isActive)
    {
        foreach (CharacterBase character in Characters.Values)
        {
            character.photonView.RPC("SetCharacterActive", RpcTarget.All, isActive);
        }
    }

    public string GetRemotePlayerName()
    {
        if (SelectedCharacter == CharacterName.Hour)
        {
            return "Milli";
        }
        else
        {
            return "Hour";
        }
    }

    public void PlayMapBgm()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string bgmKey = GetBgmKeyForScene(currentScene);

        if (!string.IsNullOrEmpty(bgmKey) && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBgm(bgmKey);
        }
        else
        {
            Debug.LogError($"[GameManager] BGM 재생 실패. 키: {bgmKey}, SoundManager 인스턴스: {SoundManager.Instance != null}");
        }
    }

    /// <summary>
    /// 맵 이름에 해당하는 BGM 이름을 반환
    /// </summary>
    private string GetBgmKeyForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Desert":
                return "desert_wind";
            case "Glacier":
                return "";
            case "Forest":
                return "";
            case "ClockTower":
                return "";
            default:
                return null; // BGM이 없는 씬
        }
    }

    public void SetLocalCharacterInput(bool enabled)
    {
        Characters.TryGetValue(SelectedCharacter, out CharacterBase character);
        if (character == null)
            return;

        character.InputHandler.enabled = enabled;
    }
}