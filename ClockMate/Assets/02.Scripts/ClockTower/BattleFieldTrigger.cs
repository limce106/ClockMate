using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFieldTrigger : MonoBehaviour
{
    HashSet<int> playersID = new HashSet<int>();
    private bool _triggered = false;

    public GameObject wall;

    private void Start()
    {
        if (!BattleManager.Instance.isCutSceneTriggerOn) return;

        wall.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!BattleManager.Instance.isCutSceneTriggerOn) return;
        if (_triggered) return;

        if(collision.collider.IsPlayerCollider())
        {
            CharacterBase characterBase = collision.gameObject.GetComponent<CharacterBase>();
            int viewID = characterBase.photonView.ViewID;

            if (!playersID.Contains(viewID))
            {
                playersID.Add(viewID);
                wall.SetActive(true);   // 한 번 전장에 올라오면 못 나감
            }
        }

        // 플레이어 둘 다 전장 위로 올라오면 컷씬 재생
        if(playersID.Count == 2)
        {
            CutsceneSyncManager.Instance.PlayForAll(
                "BattleStart",
                0f,
                () =>
                {
                    BattleManager.Instance.StartBattle();
                }
            );

            _triggered = true;
        }
    }
}
