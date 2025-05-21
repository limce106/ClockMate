using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using static Define.Character;

public class SaveManager : Singleton<SaveManager>
{
    private const string SaveFileName = "save.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary>
    /// 게임 저장.
    /// </summary>
    public void Save(int stageId)
    {
        string json = JsonUtility.ToJson(new SaveData(stageId), true);
        File.WriteAllText(SaveFilePath, json);
        
        Debug.Log($"[SaveManager] 저장 완료: {SaveFilePath}\n" +
                  $"stageId = {stageId}");
    }

    /// <summary>
    /// 저장된 게임 데이터 불러오기
    /// </summary>
    public SaveData Load()
    {
        if (SaveDataExist())
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SaveFilePath));
            Debug.Log("[SaveManager] 저장 불러오기 완료: " +
                      $"path = {SaveFilePath}" +
                      $"character = {saveData.character}, stageId = {saveData.stageId}");
            return saveData;
        }
        Debug.LogError("[SaveManager] 저장 파일 없음.");
        return null;
    }

    /// <summary>
    /// 불러올 데이터가 존재하는지 여부 반환
    /// </summary>
    public bool SaveDataExist()
    {
        return File.Exists(SaveFilePath);
    }

    public void DeleteSaveData()
    {
        File.Delete(SaveFilePath);
    }
}

[Serializable]
public class SaveData
{
    public CharacterName character;
    public int stageId;
    
    public SaveData(CharacterName character, int stageId)
    {
        this.character = character;
        this.stageId = stageId;
    }

    public SaveData(int stageId)
    {
        character = CharacterName.Hour; // 기본 캐릭터 지정
        this.stageId = stageId;
    }
}