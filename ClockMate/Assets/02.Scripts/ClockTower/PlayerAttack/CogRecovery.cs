using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CogRecovery : AttackPattern
{
    protected override void Init()
    {
        
    }

    private void Start()
    {
        SpawnCogs();
    }

    /// <summary>
    /// 톱니바퀴 스폰
    /// </summary>
    private void SpawnCogs()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        
    }
    
    public override IEnumerator Run()
    {
        if (AllCogsAligned())
        {
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
            ClearCogs();
            yield break;
        }
        
        // 제한 시간이 다 되었는지 if문으로 확인 후 아래 코드 추가
        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
        ClearCogs();
        yield break;
    }

    public override void CancelAttack() { }

    /// <summary>
    /// 모든 톱니바퀴가 홈에 맞춰졌는지
    /// </summary>
    private bool AllCogsAligned()
    {
        return true;
    }

    /// <summary>
    /// 스폰된 톱니바퀴 전부 제거
    /// </summary>
    void ClearCogs()
    {
        
    }

    /// <summary>
    /// 톱니바퀴를 스폰할 랜덤 위치 가져오기
    /// </summary>
    private Vector3 GetRandomSpawnPos()
    {
        return Vector3.zero;
    }
}
