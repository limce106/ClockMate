using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanPlatform : MonoBehaviour
{
    [SerializeField]
    private AirFan[] precedingFans;
    private bool isPlayerReached = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Milli")
        {
            if (!isPlayerReached)
            {
                isPlayerReached = true;
                ExpandFrontTriggerOfPreviousFans();
            }
        }
    }

    void ExpandFrontTriggerOfPreviousFans()
    {
        foreach (var fan in precedingFans)
        {
            fan.setting.launchDistanceThreshold += 1f;
        }
    }
}
