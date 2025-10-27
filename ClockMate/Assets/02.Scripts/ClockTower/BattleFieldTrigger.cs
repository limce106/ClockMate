using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class BattleFieldTrigger : MonoBehaviourPun
{
    HashSet<int> playersID = new HashSet<int>();
    private bool _triggered = false;

    public GameObject wall;
    public GameObject battleUI;

    private void Start()
    {
        if (!BattleManager.Instance.isCutSceneTriggerOn) return;

        wall.SetActive(false);
        battleUI.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!BattleManager.Instance.isCutSceneTriggerOn) return;
        if (_triggered) return;

        if (collision.collider.IsPlayerCollider())
        {
            CharacterBase characterBase = collision.gameObject.GetComponent<CharacterBase>();
            int viewID = characterBase.photonView.ViewID;

            if (!characterBase.photonView.IsMine) return;

            wall.SetActive(true);   // 한 번 전장에 올라오면 못 나감

            // 마스터에게 전장에 올라왔음을 보고
            photonView.RPC(nameof(RPC_ReportPlayerEntered), RpcTarget.MasterClient, viewID);
        }
    }

    [PunRPC]
    private void RPC_ShowBattleUI()
    {
        battleUI.SetActive(true);
        _triggered = true;
    }

    [PunRPC]
    private void RPC_ReportPlayerEntered(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!playersID.Contains(viewID))
        {
            playersID.Add(viewID);

            // 두 명 다 들어오면 컷씬 시작
            if (playersID.Count == 2 && !_triggered)
            {
                _triggered = true;

                CutsceneSyncManager.Instance.PlayForAll(
                    "BattleStart",
                    0f,
                    () =>
                    {
                        BattleManager.Instance.StartBattle();
                        photonView.RPC(nameof(RPC_ShowBattleUI), RpcTarget.All);
                    }
                );
            }
        }
    }
}
