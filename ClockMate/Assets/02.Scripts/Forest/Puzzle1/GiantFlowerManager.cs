using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantFlowerManager : MonoBehaviour
{
    public GiantFlower[] giantFlowers;

    public const float dropOffsetY = 5f;  // 하강 거리

    private int currentIndex = 0;
    private bool flowerLeveled = false;

    void Update()
    {
        HandleFlowerLevelCheck();
    }

    private void HandleFlowerLevelCheck()
    {
        if (currentIndex >= giantFlowers.Length)
            return;

        GiantFlower curFlower = giantFlowers[currentIndex];

        if(curFlower.IsLevel() && !flowerLeveled)
        {
            flowerLeveled = true;
            GrowSteam();
            curFlower.Lock();

            currentIndex++;
        }
    }

    private void GrowSteam()
    {
        GiantFlower nextFlower = giantFlowers[currentIndex];

        nextFlower.sideSteam.SetActive(true);
        nextFlower.steamAnimator.Play("Grow", 0, 0f);
    }
}
