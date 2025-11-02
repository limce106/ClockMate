using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIQuest : UIBase
{
    public TMP_Text quest;

    private const float duration = 10f;

    private void OnEnable()
    {
        StartCoroutine(ShowQuest());
    }

    private IEnumerator ShowQuest()
    {
        SetQuestText();
        yield return new WaitForSeconds(duration);
        UIManager.Instance.Close(this);
    }

    private void SetQuestText()
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

        quest.text = currentQuest;
    }
}
