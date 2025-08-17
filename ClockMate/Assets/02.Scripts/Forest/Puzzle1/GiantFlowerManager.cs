using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantFlowerManager : MonoBehaviour
{
    public GiantFlower[] giantFlowers;

    public const float dropOffsetY = 5f;  // �ϰ� �Ÿ�
    private float dropSpeed = 2.0f;         // �ϰ� �ӵ�

    public PressurePlateGateBlock goalGateLeft; // ������ �� ���� �� ���� ����Ʈ
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
                LowerNextFlower();
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

    private void LowerNextFlower()
    {
        Transform nextFlower = giantFlowers[currentIndex + 1].transform.parent;
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
        if (goalGateLeft == null || goalGateRight == null)
            return;

        // ����Ʈ ����
        goalGateLeft.ForceOpenDoor();
        goalGateRight.ForceOpenDoor();
    }
}
