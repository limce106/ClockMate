using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTowerEntranceTrigger : MonoBehaviour
{
    private bool _triggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        if (_triggered) return;
        if (!BattleManager.Instance.isCutSceneTriggerOn) return;

        StartCoroutine(WaitAndStartCutScene());
    }

    private IEnumerator WaitAndStartCutScene()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount != 2)
        {
            yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount == 2);
        }

        if (LoadingManager.Instance.isLoading)
        {
            yield return new WaitUntil(() => !LoadingManager.Instance.isLoading);
        }

        CutsceneSyncManager.Instance.PlayForAll(
            "ClockTowerEntrance",
            0f,
            () =>
            {
                GameManager.Instance.PlayMapBgm();
                UIManager.Instance.Show<UIMapDescription>("UIMapDescription");
            }
        );

        _triggered = true;
    }
}
