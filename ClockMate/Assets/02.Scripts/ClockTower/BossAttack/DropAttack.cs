using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropAttack : AttackPattern
{
    private int attackPendulumCount = 5;
    private bool isCanceled = false;

    [SerializeField] private float spawnOriginY = 0f;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    protected override void Init()
    {

    }

    private void SpawnPendulum()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // ������Ʈ ����
    }

    /// <summary>
    /// ������ ���� Ƚ����ŭ ���� �ݺ�
    /// </summary>
    public override IEnumerator Run()
    {
        yield break;

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

            // �̹� ������ ������Ʈ�� ��ġ�� �ʴ���
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

        var pendulumsToDestroy = FindObjectsOfType<NeedleDrop>();

        foreach (var pendulum in pendulumsToDestroy)
        {
            PhotonNetwork.Destroy(pendulum.gameObject);
        }
    }
}
