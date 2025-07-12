using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 네트워크를 사용하는 객체 풀링 시스템
/// </summary>
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

    /// <summary>
    /// 오브젝트 풀링 매니저에서는 Get과 Return만 사용하면 됨
    /// </summary>

    /// <summary>
    /// 오브젝트가 필요할 때 풀에서 가져오거나 새로 생성함
    /// </summary>
    public T Get(Vector3 position)
    {
        GameObject obj = PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity);
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// 오브젝트 사용 후 반환
    /// </summary>
    public void Return(T obj)
    {
        if (obj.TryGetComponent<PhotonView>(out PhotonView photonView) && photonView.IsMine)
            PhotonNetwork.Destroy(obj.gameObject);
    }
}
