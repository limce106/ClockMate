using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIQuest : UIBase
{
    public TMP_Text questTxt;
    public Image questName;

    private void OnEnable()
    {
        SetQuest();
    }

    private void SetQuest()
    {
        // TODO 퀘스트 이름 이미지 애셋 추가 후 주석 풀기

        string currentQuest = "";
        //Sprite questNameAsset = null;

        if(SceneManager.GetActiveScene().name == "ClockTower")
        {
            LDPlayerAttackQuest playerAttackQuest = LocalDataManager.Instance.PlayerAttackQuest.DataList.
            Where(data => data.PlayerAttackType == BattleManager.Instance.playerAttackType).First<LDPlayerAttackQuest>();

            currentQuest = GameManager.Instance.GetLocalCharacter().CompareTag("Hour") ? playerAttackQuest.HourQuest : playerAttackQuest.MilliQuest;
            //questNameAsset = Resources.Load<Sprite>(playerAttackQuest.QuestNameImgPath);
        }
        else
        {
            LDPuzzleQuest puzzleQuest = LocalDataManager.Instance.PuzzleQuest.DataList.
            Where(data => data.ID == GameManager.Instance.CurrentStage.ID).First<LDPuzzleQuest>();

            currentQuest = GameManager.Instance.GetLocalCharacter().CompareTag("Hour") ? puzzleQuest.HourQuest : puzzleQuest.MilliQuest;
            //questNameAsset = Resources.Load<Sprite>(puzzleQuest.QuestNameImgPath);
        }

        questTxt.text = currentQuest;
        //questName.sprite = questNameAsset;
    }
}
