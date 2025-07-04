using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanPlatform : MonoBehaviour
{
    [SerializeField]
    private AirFan[] _precedingFans;
    private bool _isPlayerReached = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Milli")
        {
            if (!_isPlayerReached)
            {
                _isPlayerReached = true;
                ExpandFrontTriggerOfPreviousFans();
            }
        }
    }

    void ExpandFrontTriggerOfPreviousFans()
    {
        foreach (var fan in _precedingFans)
        {
            fan.setting.launchDistanceThreshold += 1f;
        }
    }
}
