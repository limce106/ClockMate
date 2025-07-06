using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantFlowerManager : MonoBehaviour
{
    public GiantFlower[] giantFlowers;

    public const float dropOffsetY = 5f;  // 하강 거리
    private float dropSpeed = 2.0f;         // 하강 속도

    public PressurePlateGateBlock goalGate; // 마지막 꽃 수평 시 열릴 게이트

    private int currentIndex = 0;
    private bool flowerLeveled = false;

    void Update()
    {
        //HandleFlowerLevelCheck();
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
                LowerNextFlower();
                curFlower.Lock();
            }
            else
            {
                TriggerFinalAction();
            }

            currentIndex++;
        }
    }

    private bool HasNextFlower()
    {
        return currentIndex < giantFlowers.Length - 1;
    }

    private void LowerNextFlower()
    {
        Transform nextFlower = giantFlowers[currentIndex + 1].transform;
        StartCoroutine(LowerFlower(nextFlower));
    }

    IEnumerator LowerFlower(Transform nextFlower)
    {
        Vector3 start = nextFlower.position;
        Vector3 target = start + Vector3.down * dropOffsetY;

        while(Vector3.Distance(nextFlower.position, target) > 0.01f)
        {
            nextFlower.position = Vector3.Lerp(nextFlower.position, target, Time.deltaTime * dropSpeed);
            yield return null;
        }

        nextFlower.position = target;
        flowerLeveled = false;
    }

    void TriggerFinalAction()
    {
        if (goalGate == null)
            return;

        // 게이트 열기
        goalGate.ForceOpenDoor();
    }
}
