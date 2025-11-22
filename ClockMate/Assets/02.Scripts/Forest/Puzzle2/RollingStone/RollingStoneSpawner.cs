using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStoneSpawner : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public struct SpawnPointInfo
    {
        [Tooltip("스폰 위치")] public Vector3 spwanPoint;
        [Tooltip("스폰 회전값")] public Quaternion spwanRot;
        [Tooltip("스폰 주기")] public float spawnInterval;
        [Tooltip("일회성 여부")] public bool spawnOnce;
        [Tooltip("굴러가는 힘")] public float torqueForce;
        [Tooltip("사라지는 시간(초)")] public float returnTime;
    }

    public SpawnPointInfo[] spawnPoints;

    private bool spawningStarted = false;

    public NetworkObjectPool<RollingStone> rollingStonePool;
    public NetworkObjectPool<EffectWrapper> destroyStoneEffectPool;

    void Start()
    {
        if(PhotonNetwork.InRoom)
        {
            StartSpawning();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartSpawning();
    }

    void StartSpawning()
    {
        if(!PhotonNetwork.IsMasterClient || spawningStarted)
            return;

        spawningStarted = true;

        foreach (var info in spawnPoints)
        {
            StartCoroutine(SpawnLoop(info));
        }
    }

    IEnumerator SpawnLoop(SpawnPointInfo info)
    {
        if(info.spawnOnce)
        {
            SpawnStone(info.spwanPoint, info.spwanRot, info.torqueForce, info.returnTime);
            yield break;
        }

        while(true)
        {
            SpawnStone(info.spwanPoint, info.spwanRot, info.torqueForce, info.returnTime);
            yield return new WaitForSeconds(info.spawnInterval);
        }
    }

    private void SpawnStone(Vector3 point, Quaternion rot, float torque, float returnTime)
    {
        RollingStone stone = rollingStonePool.Get(point, rot);
        stone.Initialize(torque, returnTime);
    }

    public IEnumerator PlayDestroyStoneEffect(Vector3 pos, Quaternion rot)
    {
        EffectWrapper effect = destroyStoneEffectPool.Get(pos, rot);
        yield return new WaitForSeconds(effect.Particle.main.duration);
        destroyStoneEffectPool.Return(effect);
    }
}
