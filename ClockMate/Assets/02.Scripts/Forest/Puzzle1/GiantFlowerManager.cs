using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantFlowerManager : MonoBehaviour
{
    public GiantFlower[] giantFlowers;

    public const float dropOffsetY = 5f;  // 하강 거리

    public PressurePlateGateBlock goalGateLeft; // 마지막 꽃 수평 시 열릴 게이트
    public PressurePlateGateBlock goalGateRight;

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

            if(HasNextFlower())
            {
                GrowSteam();
            }
            else
            {
                TriggerFinalAction();
            }
            curFlower.Lock();

            currentIndex++;
        }
    }

    private bool HasNextFlower()
    {
        return currentIndex < giantFlowers.Length - 1;
    }

    private void GrowSteam()
    {
        GiantFlower nextFlower = giantFlowers[currentIndex];

        nextFlower.sideSteam.SetActive(true);
        nextFlower.steamAnimator.Play("Grow", 0, 0f);
    }

    void TriggerFinalAction()
    {
        if (goalGateLeft == null || goalGateRight == null)
            return;

        // 게이트 열기
        goalGateLeft.ForceOpenDoor();
        goalGateRight.ForceOpenDoor();
    }
}
