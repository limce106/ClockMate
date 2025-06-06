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
    private UILoading _uiLoading;
    private bool _isLoading;

    protected override void Init()
    {
        if (SaveManager.Instance.SaveDataExist())
        {
            // 저장 데이터가 존재하면 불러오기
            SaveData saveData = SaveManager.Instance.Load();
            CurrentStage = new BoStage(saveData.stageId);
        }
        // 캐릭터 로딩
        Characters = new Dictionary<CharacterName, CharacterBase>
        {
            { CharacterName.Hour, LoadCharacter(CharacterName.Hour) },
            { CharacterName.Milli, LoadCharacter(CharacterName.Milli) }
        };
        SetCharacterActive(false);
        _isLoading = false;
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
                LoadingManager.Instance?.StartSyncedLoading();
            }
            CurrentStage = nextStage;
        }
        else
        {
            // 엔딩 처리
        }
    }

    public void InitStageAndCharacter()
    {
        CurrentStage?.Reset();
        SetCharacterActive(true);
    }

    // 사용 안 함. 로딩은 LoadingManager에서.
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    CurrentStage?.Reset();
    //    SetCharacterActive(true);

    //    if (_uiLoading != null)
    //        UIManager.Instance.Close(_uiLoading);
    //}

    //private IEnumerator LoadSceneAsync(string sceneName)
    //{
    //    if (_isLoading) yield break;
    //    _isLoading = true;
    //    SetCharacterActive(false);

    //    // 로딩 화면 UI 활성화
    //    _uiLoading = UIManager.Instance.Show<UILoading>("UILoading");

    //    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
    //    operation.allowSceneActivation = false;

    //    // 로딩 진행도 90%까지 확인 (Unity 제한)
    //    while (operation.progress < 0.9f)
    //    {
    //        _uiLoading.UpdateLoadingProgress(operation.progress);
    //        yield return null;
    //    }

    //    // 0.9f 도달 → 준비 완료
    //    _uiLoading.UpdateLoadingProgress(1f);

    //    yield return new WaitForSeconds(0.5f); // UX용 대기

    //    operation.allowSceneActivation = true; // 실제 씬 전환
    //    _isLoading = false;
    //}

    private CharacterBase LoadCharacter(CharacterName characterName)
    {
        string path = $"Characters/{characterName}";
        CharacterBase prefab = Resources.Load<CharacterBase>(path);

        if (prefab == null)
        {
            Debug.LogError($"[GameManager] 캐릭터 프리팹 로드 실패: {path}");
            return null;
        }

        CharacterBase character = Instantiate(prefab);
        character.name = characterName.ToString();
        DontDestroyOnLoad(character.gameObject);
        
        return character;
    }

    public void SetCharacterActive(bool active)
    {
        foreach (CharacterBase character in Characters.Values)
        {
            character.gameObject.SetActive(active);
        }
    }

    public void SetSelectedCharacter(CharacterName character)
    {
        SelectedCharacter = character;
    }
}