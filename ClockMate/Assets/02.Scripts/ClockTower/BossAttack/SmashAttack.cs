using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashAttack : AttackPattern
{
    private GameObject[] battlefieldTiles;  // �ݵ�� �ڵ�� �� ���� ��. ���� ���� ���Ŀ��� �ڵ�� ���� Ÿ�� ��ġ�ϱ� ����

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
    }

    public Vector3 GetRandomRevivePos()
    {
        int random = Random.Range(0, battlefieldTiles.Length);
        return battlefieldTiles[random].transform.position;
    }
}
