using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTowerOperation : AttackPattern
{
    private GameObject _clockSpring;

    private const string ClockSpringPrefabPath = "Prefabs/ClockSpring";
    private const float SpawnPosY = 0.7f;

    protected override void Init() { }

    private void Start()
    {
        SpawnClockSpring();
    }

    private void SpawnClockSpring()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Vector3 spawnPos = BattleManager.Instance.BattleFieldCenter;
        spawnPos.y = SpawnPosY;
        _clockSpring = PhotonNetwork.Instantiate(ClockSpringPrefabPath, spawnPos, Quaternion.identity);
    }

    public override IEnumerator Run()
    {
        while (true)
        {
            if (BattleManager.Instance.IsTimeLimitEnd())
            {
                EndOperation(false);
                yield break;
            }

            if (BattleManager.Instance.GetCurrentRecovery() >= 1f)
            {
                EndOperation(true);
                yield break;
            }

            yield return null;
        }
    }

    void EndOperation(bool isSuccess)
    {
        if (_clockSpring != null && PhotonNetwork.IsMasterClient)
        {
            IAClockSpring clockSpringComp = _clockSpring.GetComponent<IAClockSpring>();
            _clockSpring.GetPhotonView().RPC(nameof(clockSpringComp.RPC_ExitControlAll), RpcTarget.All);
            PhotonNetwork.Destroy(_clockSpring);
        }

        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, isSuccess);
    }
}
