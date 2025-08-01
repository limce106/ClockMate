using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashAttack : AttackPattern
{
    private GameObject[] battlefieldTiles;  // 반드시 코드로 값 넣을 것. 상하 공격 이후에는 코드로 전장 타일 배치하기 때문

    protected override void Init()
    {

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
        yield break;
    }

    public Vector3 GetRandomRevivePos()
    {
        int random = Random.Range(0, battlefieldTiles.Length);
        return battlefieldTiles[random].transform.position;
    }
}
