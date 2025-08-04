using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class FallingAttack : AttackPattern
{
    private int attackNeedleCount = 5;
    private bool isCanceled = false;

    [SerializeField] private float spawnOriginY = 0f;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    private const int addtionalNeedleCount = 2;

    protected override void Init()
    {
        attackNeedleCount += (BattleManager.Instance.round - 1) * addtionalNeedleCount;
    }

    private void SpawnPendulum()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 오브젝트 스폰
    }

    /// <summary>
    /// 정해진 공격 횟수만큼 공격 반복
    /// </summary>
    public override IEnumerator Run()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        for(int i = 0; i < attackNeedleCount; i++)
        {
            if (isCanceled)
                yield break;
        }

        if (!isCanceled)
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
    }

    public Vector3 GetRandomSpawnPos(float y)
    {
        const float minDistance = 0.5f;

        while (true)
        {
            float x = Random.Range(BattleManager.Instance.minBattleFieldXZ.x, BattleManager.Instance.maxBattleFieldXZ.x);
            float z = Random.Range(BattleManager.Instance.minBattleFieldXZ.y, BattleManager.Instance.maxBattleFieldXZ.y);

            Vector3 randomPos = new Vector3(x, y, z);
            Vector2 randomPosXY = new Vector2(x, z);

            bool isOverlapping = false;

            // 이미 스폰된 오브젝트와 겹치지 않는지
            foreach (GameObject go in spawnedPendulums)
            {
                Vector2 exisitingPosXZ = new Vector2(go.transform.position.x, go.transform.position.z);

                if (Vector2.Distance(randomPosXY, exisitingPosXZ) <= minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
                return randomPos;
        }
    }

    public override void CancelAttack()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        isCanceled = true;

        var pendulumsToDestroy = FindObjectsOfType<FallingNeedle>();

        foreach (var pendulum in pendulumsToDestroy)
        {
            PhotonNetwork.Destroy(pendulum.gameObject);
        }
    }
}
