using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStonePoolManager : MonoBehaviour
{
    public static RollingStonePoolManager Instance;

    public int poolSize = 30;
    private string prefabPath = "Prefabs/RollingStone";

    private NetworkObjectPool<RollingStone> rollingStonePool;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        rollingStonePool = new NetworkObjectPool<RollingStone>(prefabPath, poolSize, this.transform);
    }

    public RollingStone GetStone(Vector3 pos)
    {
        return rollingStonePool.Get(pos);
    }

    public void ReturnStone(RollingStone rollingStone)
    {
        rollingStonePool.Return(rollingStone);
    }
}
