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
        public Vector3 spwanPoint;
        public float spawnInterval;
        public bool spawnOnce;
    }

    public SpawnPointInfo[] spawnPoints;

    private bool spawningStarted = false;

    public NetworkObjectPool<RollingStone> rollingStonePool;

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
            StartCoroutine(SpawnLoop(info.spwanPoint, info.spawnInterval, info.spawnOnce));
        }
    }

    IEnumerator SpawnLoop(Vector3 point, float interval, bool spawnOnce)
    {
        if(spawnOnce)
        {
            SpawnStone(point);
            yield break;
        }

        while(true)
        {
            SpawnStone(point);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnStone(Vector3 point)
    {
        RollingStone stone = rollingStonePool.Get(point, Quaternion.identity);
    }
}
