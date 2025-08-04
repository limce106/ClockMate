using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectPool<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPun
{
    [SerializeField] private string prefabPath;
    [SerializeField] private int initialPoolSize = 10;

    private List<T> pool = new List<T>();
    private int poolSize;

    public static NetworkObjectPool<T> Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!PhotonNetwork.IsMasterClient)
            return;

        InitPool();
    }

    public void Initialize(string prefabPath, int initialSize, Transform parent = null)
    {
        this.prefabPath = prefabPath;
        this.initialPoolSize = initialSize;

        if (!PhotonNetwork.IsMasterClient) return;
        InitPool();
    }


    private void InitPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = PhotonNetwork.Instantiate(prefabPath, Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Prefab at {prefabPath} does not have component {typeof(T)}");
                continue;
            }
            pool.Add(component);
        }
    }

    // Ǯ���� ������Ʈ ������
    public T Get(Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient)
            return null;

        T obj = GetInactiveObject();
        if (obj == null)
        {
            GameObject newObj = PhotonNetwork.Instantiate(prefabPath, Vector3.zero, Quaternion.identity);
            obj = newObj.GetComponent<T>();
            pool.Add(obj);
        }

        int index = pool.IndexOf(obj);
        photonView.RPC(nameof(RPC_ActivateObject), RpcTarget.All, index, position);
        return obj;
    }

    // ������Ʈ Ǯ�� ��ȯ�ϱ�
    public void Return(T obj)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int index = pool.IndexOf(obj);
        if (index < 0) return;

        photonView.RPC(nameof(RPC_DeactivateObject), RpcTarget.All, index);
    }

    private T GetInactiveObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.gameObject.activeSelf)
                return obj;
        }
        return null;
    }

    [PunRPC]
    public void RPC_ActivateObject(int index, Vector3 position)
    {
        if (index < 0 || index >= pool.Count) return;

        T obj = pool[index];
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
    }

    [PunRPC]
    public void RPC_DeactivateObject(int index)
    {
        if (index < 0 || index >= pool.Count) return;

        T obj = pool[index];
        obj.gameObject.SetActive(false);
    }
}
