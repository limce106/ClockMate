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

    public void UpdateQuest()
    {
        string currentQuest = "";

        if(SceneManager.GetActiveScene().name == "ClockTower")
        {
            LDPlayerAttackQuest playerAttackQuest = LocalDataManager.Instance.PlayerAttackQuest.DataList.
            Where(data => data.PlayerAttackType == BattleManager.Instance.playerAttackType).First<LDPlayerAttackQuest>();

            currentQuest = GameManager.Instance.GetLocalCharacter().CompareTag("Hour") ? playerAttackQuest.HourQuest : playerAttackQuest.MilliQuest;
        }
        else
        {
            LDPuzzleQuest puzzleQuest = LocalDataManager.Instance.PuzzleQuest.DataList.
            Where(data => data.ID == GameManager.Instance.CurrentStage.ID).First<LDPuzzleQuest>();

            currentQuest = GameManager.Instance.GetLocalCharacter().CompareTag("Hour") ? puzzleQuest.HourQuest : puzzleQuest.MilliQuest;
        }

        questTxt.text = currentQuest;

        SoundManager.Instance.PlaySfx(key: "quest", pos: null, volume: 0.3f);
    }
}
