using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class RollingStoneSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPointInfo
    {
        public Transform spwanPoint;
        public float spawnInterval;
        public bool spawnOnce;
    }

    public SpawnPointInfo[] spawnPoints;

    void Start()
    {
        foreach (var info in spawnPoints)
        {
            StartCoroutine(SpawnLoop(info.spwanPoint, info.spawnInterval, info.spawnOnce));
        }
    }

    IEnumerator SpawnLoop(Transform point, float interval, bool spawnOnce)
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

    private void SpawnStone(Transform point)
    {
        var stone = RollingStonePoolManager.Instance.GetStone();
        stone.transform.position = point.position;
        stone.transform.rotation = Quaternion.identity;
    }
}
