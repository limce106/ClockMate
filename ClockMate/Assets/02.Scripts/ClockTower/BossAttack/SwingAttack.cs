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

    // ���� ����(������Ʈ ���� ����) ����
    [SerializeField] private Vector2 attackOriginXY = Vector2.zero;
    [SerializeField] private float attackZMin = 0f;
    [SerializeField] private float attackZMax = 0f;

    // ���� ȸ�� ���� ����
    [SerializeField] private float avoidZMin = 0f;
    [SerializeField] private float avoidZMax = 0f;

    private List<GameObject> spawnedPendulums = new List<GameObject>();

    // �ּ�/�ִ� ���� Ƚ��
    private const int minSpawnNum = 1;
    private const int maxSpawnNum = 4;

    private const int additionalAttackCount = 3; // ���� ������ ������ �þ ���� Ƚ��
    private readonly float[] startAngles = { -60f, 60f };

    protected override void Init()
    {
        attackCount += (BattleManager.Instance.round - 1) * additionalAttackCount;
    }

    /// <summary>
    /// �� ���ݿ� ���� �ð� �� ����
    /// </summary>
    private void SpawnPendulum(float startAngle)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        spawnedPendulums.Clear();
        // �� ���ݴ� ������ �ð� �� ����
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
    /// ���� ���� ���� �� ���� ���� ���� ��ȯ
    /// �̹� ������ ������Ʈ�� ��ġ�� �ʰ�, ȸ�� ������ ����
    /// </summary>
    private Vector3 GetRandomSpawnPos()
    {
        const float minDistance = 0.5f;

        while(true)
        {
            float x = attackOriginXY.x;
            float y = attackOriginXY.y;
            float z = Random.Range(attackZMin, attackZMax);

            Vector3 randomPos = new Vector3(x, y, z);
            bool isOverlapping = false;

            // ȸ�� ���� ��������
            bool isInAvoidableZone = randomPos.z >= avoidZMin && randomPos.z <= avoidZMax;
            if (isInAvoidableZone)
                continue;

            // �̹� ������ ������Ʈ�� ��ġ�� �ʴ���
            foreach (GameObject go in spawnedPendulums)
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
    /// ������ ��� �ð� �� ���ڿ ����
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

        // ��� �ð��߰� ���ŵ� ������ ���
        while (disabledCount < spawnedPendulums.Count)
            yield return null;
    }

    /// <summary>
    /// ������ ���� Ƚ����ŭ ���� �ݺ�
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