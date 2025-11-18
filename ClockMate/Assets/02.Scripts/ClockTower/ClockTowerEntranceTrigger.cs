using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ClockTowerEntranceTrigger : MonoBehaviourPun
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
                photonView.RPC(nameof(RPC_PlayBGMAndShowMapDescription), RpcTarget.All);
            }
        );

        _triggered = true;
    }

    [PunRPC]
    private void RPC_PlayBGMAndShowMapDescription()
    {
        //GameManager.Instance.PlayMapBgm();
        
        PuzzleHUD puzzleHUD = GameObject.FindAnyObjectByType<PuzzleHUD>();
        puzzleHUD?.ShowMapDescription();
    }
}
