using System.Collections.Generic;
using Define;
using UnityEngine;
using static Define.Character;

public class BoStage
{
    public int ID { get; private set; }
    public Map.MapName Map { get; private set; }
    
    public int MaxReviveCount { get; private set; }
    public int LeftReviveCount { get; private set; }
    public bool IsReviveImpossible => LeftReviveCount < 0;
    public BoStage NextStage {get; private set;}
    
    public Dictionary<CharacterName, Vector3> LoadPositions { get; set; }

    public BoStage(int id)
    {
        LDStage ldStage = LocalDataManager.Instance.Stage[id];
        ID = ldStage.ID;
        Map = ldStage.Map;
        MaxReviveCount = ldStage.ReviveCount;
        LeftReviveCount = ldStage.ReviveCount;
        LoadPositions = new Dictionary<CharacterName, Vector3>
        {
            [CharacterName.Hour] = new Vector3(ldStage.HourLoadPos[0], ldStage.HourLoadPos[1], ldStage.HourLoadPos[2]),
            [CharacterName.Milli] = new Vector3(ldStage.MilliLoadPos[0], ldStage.MilliLoadPos[1], ldStage.MilliLoadPos[2])
        };
        if (ldStage.NextStageID > 0)
        {
            NextStage = new BoStage(ldStage.NextStageID);
        }
    }

    public void ReduceReviveCount()
    {
        LeftReviveCount--;
    }

    public void Reset()
    {
        // 각 캐릭터를 로드 위치로 이동
        foreach (CharacterName character in LoadPositions.Keys)
        {
            GameManager.Instance.Characters[character].transform.position = LoadPositions[character];
        }

        // 블럭 초기화
        ResetTestManager.Instance.ResetAllBlocks();

        // 부활 가능 횟수 초기화
        //LeftReviveCount = MaxReviveCount;
    }
}