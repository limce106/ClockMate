using System.Collections;
using System.Collections.Generic;
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
    private UISetting _uiSetting;

    protected override void Init()
    {
        Characters = new Dictionary<CharacterName, CharacterBase>();
    }

    private void Start()
    {
        _rpcManager = RPCManager.Instance;

        if(!Application.isEditor)
            Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // 전투 진행 중이면 설정창 열기 불가
            if (SceneManager.GetActiveScene().name == "ClockTower" && BattleManager.Instance.isInBattle) return;

            // 컷씬 영상 재생 중이면 설정창 열기 불가
            if (CutsceneSyncManager.Instance.IsBusy) return;

            ToggleSetting();
        }
    }

    /// <summary>
    /// 설정창 On/Off
    /// </summary>
    private void ToggleSetting()
    {
        if(_uiSetting != null && UIManager.Instance.IsOnScreen(_uiSetting))
        {
            UIManager.Instance.Close(_uiSetting);
        }
        else
        {
            _uiSetting = UIManager.Instance.Show<UISetting>("UISetting");
        }

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    /// <summary>
    /// 기존에 저장해둔 세이브 데이터를 불러와 현재 스테이지로 설정한다.
    /// 이어하기를 선택할 시 마스터가 호출한다
    /// </summary>
    public void SetStageWithExistingData()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (!SaveManager.Instance.SaveDataExist()) return;
        
        // 저장 데이터가 존재하면 불러오기
        SaveData saveData = SaveManager.Instance.Load();
        SetSelectedCharacter(saveData.character);
        Debug.Log($"저장된 캐릭터 불러와짐: {SelectedCharacter}");
        CharacterName otherCh = saveData.character == CharacterName.Hour ? CharacterName.Milli : CharacterName.Hour;
        _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_SetSelectedCharacter), RpcTarget.Others, (int) otherCh);
        _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_MoveToStage), RpcTarget.All, saveData.stageId);
    }
    
    /// <summary>
    /// 새 게임 시작 시 호출한다.
    /// 현재 스테이지를 1로 설정하고 저장한다.
    /// 기존 저장 데이터가 있다면 덮어쓴다.
    /// </summary>
    public void CreateNewSaveDataAndSetStage()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // 저장된 데이터가 없으면 (새 게임이면)
        SaveManager.Instance.SaveNewGame(SelectedCharacter); // 사막 맵 stage 1으로 저장
        _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_MoveToStage), RpcTarget.All, 1);
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
            SaveManager.Instance.SaveStage(nextStage.ID); // 진행 상태 저장
            _rpcManager.photonView.RPC(nameof(_rpcManager.RPC_MoveToStage), RpcTarget.All, nextStage.ID);
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
    }

    public void SetAllCharactersActive(bool isActive)
    {
        foreach (CharacterBase character in Characters.Values)
        {
            character.photonView.RPC(nameof(character.SetCharacterActive), RpcTarget.All, isActive);
        }
    }

    public string GetRemotePlayerName()
    {
        return SelectedCharacter == CharacterName.Hour ? "Milli" : "Hour";
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
            Debug.LogError($"[GameManager] BGM 재생 실패. Bgm 키: {bgmKey}, SoundManager 인스턴스: {SoundManager.Instance != null}");
        }
    }

    /// <summary>
    /// 맵 이름에 해당하는 BGM 이름을 반환
    /// </summary>
    private string GetBgmKeyForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "TitleMatch":
                return "title_bgm";
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
    public void SetLocalCharacterInput(bool enabled)
    {
        Characters.TryGetValue(SelectedCharacter, out CharacterBase character);
        if (character == null)
            return;

        character.InputHandler.enabled = enabled;
    }

    public CharacterBase GetLocalCharacter()
    {
        if (Characters == null) return null;

        if (Characters.Count == 2)
            return Characters[SelectedCharacter];
        else
            return null;
    }
}