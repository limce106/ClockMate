using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Define.Battle;

public class PendulumAttack : AttackPattern
{
    public string pendulmnPrefabPath = "Prefabs/Pendulum";
    public float spawnDuration = 3f;

    public Vector3 minPos = Vector3.zero;
    public Vector3 maxPos = Vector3.zero;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    private const int minSpawnNum = 1;
    private const int maxSpawnNum = 4;
    private readonly float[] startAngles = { -60f, 60f };

    protected override void Init() { }

    protected override void SpawnObj()
    {
        int spawnCount = Mathf.Clamp(BattleManager.Instance.round, minSpawnNum, maxSpawnNum);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = GetRandomSpawnPos();
            float startAngleZ = startAngles[i % 2];

            GameObject pendulum = PhotonNetwork.Instantiate(pendulmnPrefabPath, pos, Quaternion.identity);
            pendulum.transform.localRotation = Quaternion.Euler(0, 0, startAngleZ);
            spawnedPendulums.Add(pendulum);
        }
    }

    private Vector3 GetRandomSpawnPos()
    {
        const float minDistance = 1.5f;

        while(true)
        {
            float x = minPos.x;
            float y = minPos.y;
            float z = Random.Range(minPos.z, maxPos.z);

            Vector3 randomPos = new Vector3(x, y, z);

            bool isOverlapping = false;

            foreach(GameObject go in spawnedPendulums)
            {
                if(Vector3.Distance(go.transform.position, randomPos) < minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
                return randomPos;
        }
    }

    public override IEnumerator Run()
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

    public override bool IsSuccess()
    {
        // TODO 두 플레이어가 죽으면 false 처리
        return true;
    }
}
