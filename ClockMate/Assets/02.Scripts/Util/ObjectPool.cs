using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Stack<T> pool = new Stack<T>();
    private readonly T prefab;  // ������ ���� ������
    private readonly Transform parent;  // ������ ������Ʈ�� ��Ƴ��� �θ�(������)

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for(int i = 0; i < initialSize; i++)
        {
            T instance = GameObject.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            pool.Push(instance);
        }
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Pop() : GameObject.Instantiate(prefab, parent);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Push(obj);
    }
}
