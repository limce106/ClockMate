using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStonePoolManager : MonoBehaviour
{
    public static RollingStonePoolManager Instance;

    public RollingStone stonePrefab;
    public int poolSize = 30;

    private ObjectPool<RollingStone> rollingStonePool;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rollingStonePool = new ObjectPool<RollingStone>(stonePrefab, poolSize, this.transform);
    }

    public RollingStone GetStone()
    {
        return rollingStonePool.Get();
    }

    public void ReturnStone(RollingStone rollingStone)
    {
        rollingStonePool.Return(rollingStone);
    }
}
