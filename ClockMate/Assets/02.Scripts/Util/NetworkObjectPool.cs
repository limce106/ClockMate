using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectPool<T> : IPunPrefabPool where T : Component
{
    private readonly Stack<T> pool = new Stack<T>();
    private readonly string prefabPath;
    private readonly Transform parent;

    public NetworkObjectPool(string prefabPath, int initialSize, Transform parent = null)
    {
        this.prefabPath = prefabPath;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            GameObject instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            instance.gameObject.SetActive(false);
            pool.Push(instance.GetComponent<T>());
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Pop().gameObject;
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            obj = GameObject.Instantiate(prefab, parent);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(false);

        return obj;
    }

    public void Destroy(GameObject gameObject)
    {
        gameObject.SetActive(false);
        T component = gameObject.GetComponent<T>();
        pool.Push(component);
    }

    public T Get(Vector3 position)
    {
        GameObject obj = PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity);
        return obj.GetComponent<T>();
    }

    public void Return(T obj)
    {
        if (obj.TryGetComponent<PhotonView>(out PhotonView photonView) && photonView.IsMine)
            PhotonNetwork.Destroy(obj.gameObject);
    }
}
