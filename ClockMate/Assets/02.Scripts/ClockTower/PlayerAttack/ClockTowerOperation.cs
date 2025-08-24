using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTowerOperation : AttackPattern
{
    private GameObject _clockSpring;

    private const string ClockSpringPrefabPath = "Prefabs/ClockSpring";

    protected override void Init() { }

    private void Start()
    {
        SpawnClockSpring();
    }

    private void SpawnClockSpring()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _clockSpring = PhotonNetwork.Instantiate(ClockSpringPrefabPath, BattleManager.Instance.BattleFieldCenter, Quaternion.identity);
    }

    public override IEnumerator Run()
    {
        while (true)
        {
            if (BattleManager.Instance.IsTimeLimitEnd() || BattleManager.Instance.GetCurrentRecovery() >= 1f)
            {
                EndOperation(false);
                yield break;
            }

            if (BattleManager.Instance.GetCurrentRecovery() >= 1f)
            {
                EndOperation(true);
                yield break;
            }
        }
    }

    void EndOperation(bool isSuccess)
    {
        if (_clockSpring != null)
            PhotonNetwork.Destroy(_clockSpring);

        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, isSuccess);
    }
}
