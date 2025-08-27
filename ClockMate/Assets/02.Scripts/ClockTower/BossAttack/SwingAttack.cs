using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Define.Battle;

public class SwingAttack : AttackPattern
{
    private int attackCount = 5;
    private bool isCanceled = false;

    // 공격 가능(오브젝트 스폰 가능) 구간
    [SerializeField] private float attackOriginY = 0f;

    // 공통 회피 가능 구간
    [SerializeField] private float avoidRadiusMin = 0f;
    [SerializeField] private float avoidRadiusMax = 0f;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    // 최소/최대 스폰 횟수
    private const int minSpawnNum = 1;
    private const int maxSpawnNum = 4;

    private const int additionalAttackCount = 3; // 라운드 증가할 때마다 늘어날 공격 횟수
    private readonly float[] startAngles = { -60f, 60f };

    protected override void Init()
    {
        attackCount += (BattleManager.Instance.round - 1) * additionalAttackCount;
    }

    /// <summary>
    /// 한 공격에 사용될 시계 추 스폰
    /// </summary>
    private void SpawnPendulum(float startAngle)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        spawnedPendulums.Clear();
        // 한 공격당 스폰될 시계 추 개수
        int spawnNum = Mathf.Clamp(BattleManager.Instance.round, minSpawnNum, maxSpawnNum);

        for (int i = 0; i < spawnNum; i++)
        {
            Vector3 pos = GetRandomSpawnPos();
            Quaternion rotation = Quaternion.Euler(0, 0, startAngle);

            SwingPendulum pendulum = BattleManager.Instance.pendulumPool.Get(pos, rotation);
            spawnedPendulums.Add(pendulum.gameObject);
        }
    }

    /// <summary>
    /// 공격 가능 구간 내 랜덤 스폰 지점 반환
    /// 이미 스폰된 오브젝트와 겹치지 않게, 회피 구간은 제외
    /// </summary>
    private Vector3 GetRandomSpawnPos()
    {
        const float minDistance = 0.5f;

        while (true)
        {
            // 랜덤 위치 생성
            float r = BattleManager.Instance.battleFieldRadius * Mathf.Sqrt(Random.value);
            float angle = Random.value * 360f;

            float x = BattleManager.Instance.BattleFieldCenter.x + r * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = BattleManager.Instance.BattleFieldCenter.z + r * Mathf.Sin(angle * Mathf.Deg2Rad);
            float y = attackOriginY;

            Vector3 randomPos = new Vector3(x, y, z);

            // 회피 구간 확인. 회피 구간이 원형 전장의 중심으로부터 특정 반지름을 벗어난 도넛 모양이라고 가정
            float distanceToCenter = Vector3.Distance(randomPos, BattleManager.Instance.BattleFieldCenter);
            bool isInAvoidableZone = distanceToCenter >= avoidRadiusMin && distanceToCenter <= avoidRadiusMax;

            if (isInAvoidableZone)
                continue;

            bool isOverlapping = false;
            foreach (GameObject go in spawnedPendulums)
            {
                if (Vector3.Distance(go.transform.position, randomPos) <= minDistance)
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
    /// 스폰된 모든 시계 추 진자운동 시작
    /// </summary>
    private IEnumerator MovePendulum()
    {
        int disabledCount = 0;

        foreach (GameObject pendulumGO in spawnedPendulums)
        {
            SwingPendulum pendulum = pendulumGO.GetComponent<SwingPendulum>();
            PhotonView pv = pendulumGO.GetComponent<PhotonView>();

            pv.RPC("StartPendulum", RpcTarget.All);
            pendulum.OnPendulumDisabled += (p) => disabledCount++;
        }

        // 모든 시계추가 제거될 때까지 대기
        while (disabledCount < spawnedPendulums.Count)
            yield return null;
    }

    /// <summary>
    /// 정해진 공격 횟수만큼 공격 반복
    /// </summary>
    public override IEnumerator Run()
    {
        for (int i = 0; i < attackCount; i++)
        {
            if (isCanceled)
                yield break;

            SpawnPendulum(startAngles[i % 2]);
            yield return StartCoroutine(MovePendulum());

            yield return new WaitForSeconds(1f);
        }

        if(!isCanceled)
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
    }

    public override void CancelAttack()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        isCanceled = true;

        var pendulumsToReturn = new List<GameObject>(spawnedPendulums);
        foreach (GameObject pendulumGO in pendulumsToReturn)
        {
            SwingPendulum pendulum = pendulumGO.GetComponent<SwingPendulum>();
            BattleManager.Instance.pendulumPool.Return(pendulum);
        }
    }
}