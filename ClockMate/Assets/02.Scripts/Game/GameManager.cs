using System.Collections.Generic;
using System.Net;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Character;

public class GameManager : MonoSingleton<GameManager>
{
    [field: SerializeField] public CharacterName SelectedCharacter { get; private set; } = CharacterName.Hour;
    public Dictionary<CharacterName, CharacterBase> Characters { get; private set; }
    public BoStage CurrentStage { get; private set; }

    private RPCManager _rpcManager;
    private UIStageDebugLoader _uiStageDebugLoader;

    protected override void Init()
    {
        Characters = new Dictionary<CharacterName, CharacterBase>();
        _rpcManager = RPCManager.Instance;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleStageLoader();
        }
    }

    /// <summary>
    /// 기존에 저장해둔 세이브 데이터를 불러와 현재 스테이지로 설정한다.
    /// 이어하기를 선택할 시 마스터가 호출한다
    /// </summary>
    public void LoadExistingSaveData()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (!SaveManager.Instance.SaveDataExist()) return;
        
        // 저장 데이터가 존재하면 불러오기
        SaveData saveData = SaveManager.Instance.Load();
        _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_SyncStage), RpcTarget.All, saveData.stageId);
    }
    
    /// <summary>
    /// 새 게임 시작 시 호출한다.
    /// 현재 스테이지를 1로 설정하고 저장한다.
    /// 기존 저장 데이터가 있다면 덮어쓴다.
    /// </summary>
    public void CreateNewSaveData()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // 저장된 데이터가 없으면 (새 게임이면)
        SaveManager.Instance.Save(1); // 사막 맵 stage 1으로 저장
        _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_SyncStage), RpcTarget.All, 1);
    }

    /// <summary>
    /// 스테이지 클리어 시 마스터만 호출한다.
    /// 마스터는 스테이지 클리어 상태를 저장하고
    /// 마스터와 클라이언트의 CurrentStage를 마스터 기준으로 동일하게 업데이트한다. 
    /// </summary>
    public void StageComplete()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        BoStage nextStage = CurrentStage.NextStage;
        if (nextStage != null)
        {
            // 다음 스테이지 존재하는 경우
            
            SaveManager.Instance.Save(nextStage.ID); // 진행 상태 저장
            if (nextStage.Map != CurrentStage.Map)
            {
                // 이번 맵의 마지막 스테이지일 경우 다음 맵으로 이동
                _rpcManager.photonView.RPC(
                    nameof(_rpcManager.RPC_MoveToMap), RpcTarget.All, nextStage.Map.ToString()
                );
            }

            _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_SyncStage), RpcTarget.All, nextStage.ID);
        }
    }


    /// <summary>
    /// stageID로 CurrentStage를 업데이트한다.
    /// </summary>
    public void SetCurrentStage(int stageID)
    {
        CurrentStage = new BoStage(stageID);   
    }
    
    /// <summary>
    /// 네트워크에 연결된 상태이고 마스터라면
    /// 마스터와 클라이언트 모두 현재 스테이지를 초기화하고
    /// 플레이어 캐릭터를 활성화 상태로 만든다.
    /// </summary>
    public void ResetStage()
    {
        if (!NetworkManager.Instance.IsInRoomAndReady())
        {
            _rpcManager.RPC_SyncReset();
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient) return;
            _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_SyncReset), RpcTarget.All);
        }

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

            RegisterCharacter(SelectedCharacter, character);

            return true;
        }

        Debug.LogError($"[GameManager] 캐릭터 프리팹({SelectedCharacter}) 로드 실패");
        return false;
    }

    public void SetSelectedCharacter(CharacterName character)
    {
        SelectedCharacter = character;
    }

    public void RegisterCharacter(CharacterName character, CharacterBase characterBase)
    {
        Characters[character] = characterBase;
        if (NetworkManager.Instance.IsInRoomAndReady())
        {
            RPCManager.Instance.photonView.RPC("RPC_RegisterCharacter", 
                RpcTarget.Others, character, characterBase.photonView.ViewID);

        }
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
        string envKey = GetEnvKeyForScene(currentScene);
        float envVolume = GetEnvVolumeForScene(currentScene);

        if (!string.IsNullOrEmpty(bgmKey) && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBgm(bgmKey);
        }
        else
        {
            Debug.LogError($"[GameManager] BGM 재생 실패. Bgm 키: {bgmKey}, SoundManager 인스턴스: {SoundManager.Instance != null}");
        }

        if (!string.IsNullOrEmpty(envKey) && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(key: envKey, loop: true, pos: null, volume: envVolume);
        }
        else
        {
            Debug.LogError($"[GameManager] BGM 재생 실패. Env 키: {envKey}, SoundManager 인스턴스: {SoundManager.Instance != null}");
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
                return "desert_bgm";
            case "Glacier":
                return "glacier_bgm";
            case "Forest":
                return "forest_bgm";
            case "ClockTower":
                return "clocktower_bgm";
            default:
                return null; // BGM이 없는 씬
        }
    }

    /// <summary>
    /// 맵 이름에 해당하는 환경음 반환
    /// </summary>
    private string GetEnvKeyForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Desert":
                return "desert_wind";
            case "Glacier":
                return "glacier_storm";
            case "Forest":
                return "forest_rain";
            case "ClockTower":
                return "";
            default:
                return null; // 환경음이 없는 씬
        }
    }

    /// <summary>
    /// 맵 이름에 해당하는 환경음 소리 크기 반환
    /// </summary>
    private float GetEnvVolumeForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Desert":
                return 1f;
            case "Glacier":
                return 1f;
            case "Forest":
                return 0.05f;
            case "ClockTower":
                return 1f;
            default:
                return 1f;
        }
    }

    public void SetLocalCharacterInput(bool enabled)
    {
        Characters.TryGetValue(SelectedCharacter, out CharacterBase character);
        if (character == null)
            return;

        character.InputHandler.enabled = enabled;
    }

    public CharacterBase GetLocalCharacter()
    {
        return Characters[SelectedCharacter];
    }

    /// <summary>
    /// 개발자용 치트키 UI 토글
    /// </summary>
    public void ToggleStageLoader()
    {
        if(_uiStageDebugLoader == null)
        {
            _uiStageDebugLoader = UIManager.Instance.Show<UIStageDebugLoader>("UIStageDebugLoader");
        }
        else
        {
            UIManager.Instance.Close(_uiStageDebugLoader);
            _uiStageDebugLoader = null;
        }
    }
}