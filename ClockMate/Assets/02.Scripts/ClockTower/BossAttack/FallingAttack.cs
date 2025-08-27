using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class FallingAttack : AttackPattern
{
    private int attackClockHandCount = 5;
    private bool isCanceled = false;

    [SerializeField] private float spawnOriginY = 0f;

    private List<GameObject> spawnedClockHands = new List<GameObject>();

    private const int addtionalCount = 2;
    private const float spawnDelay = 1f;

    protected override void Init()
    {
        attackClockHandCount += (BattleManager.Instance.round - 1) * addtionalCount;
    }

    /// <summary>
    /// 정해진 공격 횟수만큼 공격 반복
    /// </summary>
    public override IEnumerator Run()
    {
        for (int i = 0; i < attackClockHandCount; i++)
        {
            if (isCanceled)
                yield break;

            Vector3 pos = GetRandomSpawnPos(spawnOriginY);

            FallingClockHand clockHand = BattleManager.Instance.clockhandPool.Get(pos, Quaternion.identity);
            spawnedClockHands.Add(clockHand.gameObject);

            clockHand.OnFallingClockHandDisabled += (n) => spawnedClockHands.Remove(n);

            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitUntil(() => spawnedClockHands.Count == 0);

        if (!isCanceled)
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
    }

    public Vector3 GetRandomSpawnPos(float y)
    {
        const float minDistance = 0.5f;
        float battleFieldRadius = BattleManager.Instance.battleFieldRadius; // 원형 전장의 반지름

        while (true)
        {
            // 랜덤 위치 생성
            float r = battleFieldRadius * Mathf.Sqrt(Random.value);
            float angle = Random.value * 360f;

            float x = r * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = r * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 randomPos = new Vector3(BattleManager.Instance.BattleFieldCenter.x + x, y, BattleManager.Instance.BattleFieldCenter.z + z);

            Vector2 randomPosXZ = new Vector2(randomPos.x, randomPos.z);

            bool isOverlapping = false;

            foreach (GameObject go in spawnedClockHands)
            {
                Vector2 existingPosXZ = new Vector2(go.transform.position.x, go.transform.position.z);

                if (Vector2.Distance(randomPosXZ, existingPosXZ) <= minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
            {
                return randomPos;
            }
        }
    }

    public override void CancelAttack()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        isCanceled = true;

        var clockHandsToDestroy = FindObjectsOfType<FallingClockHand>();

        foreach (var clockHand in clockHandsToDestroy)
        {
            BattleManager.Instance.clockhandPool.Return(clockHand);
        }
    }
}
