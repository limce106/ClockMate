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
                continue;
            }
            pool.Add(component);
        }
    }

    // 풀에서 오브젝트 꺼내기
    public T Get(Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.IsMasterClient)
            return null;

        T obj = GetInactiveObject();
        if (obj == null)
        {
            GameObject newObj = PhotonNetwork.Instantiate(prefabPath, position, rotation);
            obj = newObj.GetComponent<T>();
            pool.Add(obj);
        }

        int viewID = obj.photonView.ViewID;

        photonView.RPC(nameof(RPC_ActivateObject), RpcTarget.All, viewID, position, rotation);

        return obj;
    }

    // 오브젝트 풀에 반환하기
    public void Return(T obj)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int viewID = obj.photonView.ViewID;

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(RPC_DeactivateObject), RpcTarget.All, viewID);
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
    public void RPC_ActivateObject(int viewID, Vector3 position, Quaternion rotation)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null)
        {
            return;
        }

        T obj = view.GetComponent<T>();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
    }

    [PunRPC]
    public void RPC_DeactivateObject(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null)
        {
            return;
        }

        T obj = view.GetComponent<T>();
        obj.gameObject.SetActive(false);
    }

}
