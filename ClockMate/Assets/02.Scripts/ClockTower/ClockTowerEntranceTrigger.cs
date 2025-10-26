using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTowerEntranceTrigger : MonoBehaviour
{
    private bool _triggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (_triggered) return;

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
                UIManager.Instance.Show<UIMapDescription>("UIMapDescription");
            }
        );

        _triggered = true;
    }
}
