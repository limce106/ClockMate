using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTowerEntranceTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(WaitAndStartCutScene());
    }

    private IEnumerator WaitAndStartCutScene()
    {
        yield return new WaitUntil(() => !LoadingManager.Instance._isLoading);

        CutsceneSyncManager.Instance.PlayForAll(
            "ClockTowerEntrance",
            0f,
            () =>
            {
                GameManager.Instance.PlayMapBgm();
            }
        );
    }
}
