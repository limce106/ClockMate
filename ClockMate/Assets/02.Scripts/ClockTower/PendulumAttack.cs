using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Define.Battle;

public class PendulumAttack : AttackPattern
{
    public string pendulmnPrefabPath = "Prefabs/Pendulum";
    private int spawnCount = 5;

    public Vector3 minPos = Vector3.zero;
    public Vector3 maxPos = Vector3.zero;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    private const int minSpawnNum = 1;
    private const int maxSpawnNum = 4;

    private const int additionalSpawnCount = 3;
    private readonly float[] startAngles = { -60f, 60f };

    protected override void Init()
    {
        spawnCount += (BattleManager.Instance.round - 1) * additionalSpawnCount;
    }

    private void SpawnPendulum(float startAngle)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        spawnedPendulums.Clear();
        // 한 진자운동에 스폰될 시계 추 개수
        int spawnNum = Mathf.Clamp(BattleManager.Instance.round, minSpawnNum, maxSpawnNum);

        for (int i = 0; i < spawnNum; i++)
        {
            Vector3 pos = GetRandomSpawnPos();

            Quaternion rotation = Quaternion.Euler(0, 0, startAngle);
            GameObject pendulum = PhotonNetwork.Instantiate(pendulmnPrefabPath, pos, rotation);
            spawnedPendulums.Add(pendulum);
        }
    }

    private Vector3 GetRandomSpawnPos()
    {
        const float minDistance = 0.5f;

        while(true)
        {
            float x = minPos.x;
            float y = minPos.y;
            float z = Random.Range(minPos.z, maxPos.z);

            Vector3 randomPos = new Vector3(x, y, z);

            bool isOverlapping = false;

            foreach(GameObject go in spawnedPendulums)
            {
                if(Vector3.Distance(go.transform.position, randomPos) <= minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
                return randomPos;
        }
    }

    /// <summary>
    /// 스폰된 시계 추 진자운동 시작
    /// </summary>
    private IEnumerator MovePendulum()
    {
        int destroyedCount = 0;

        foreach (GameObject pendulumGO in spawnedPendulums)
        {
            Pendulum pendulum = pendulumGO.GetComponent<Pendulum>();
            PhotonView pv = pendulumGO.GetComponent<PhotonView>();

            pv.RPC("StartPendulum", RpcTarget.All);
            pendulum.OnPendulumDestroyed += (p) => destroyedCount++;
        }

        // 모든 시계추가 제거될 때까지 대기
        while (destroyedCount < spawnedPendulums.Count)
            yield return null;
    }

    public override IEnumerator Run()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnPendulum(startAngles[i % 2]);
            yield return StartCoroutine(MovePendulum());

            yield return new WaitForSeconds(1f);
        }
    }
}