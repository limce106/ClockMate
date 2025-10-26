using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectPool<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPun
{
    [SerializeField] private string prefabPath;
    [SerializeField] private int initialPoolSize = 10;

    private List<T> pool = new List<T>();
    private int poolSize;

    private bool isInitPool = false;

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

        if(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            InitPool();
            isInitPool = true;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!isInitPool)
            InitPool();
    }


    private void InitPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = PhotonNetwork.Instantiate(prefabPath, Vector3.zero, Quaternion.identity);
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                continue;
            }
            pool.Add(component);
            photonView.RPC(nameof(RPC_DeactivateObject), RpcTarget.All, obj.GetPhotonView().ViewID);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트 꺼내기
    /// </summary>
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

    /// <summary>
    /// 오브젝트 풀에 반환하기
    /// </summary>
    public void Return(T obj)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int viewID = obj.photonView.ViewID;
        photonView.RPC(nameof(RPC_DeactivateObject), RpcTarget.All, viewID);
    }

    public void ReturnAll()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        int[] viewIDs = new int[pool.Count];
        for (int i = 0; i < pool.Count; i++)
        {
            pool[i].gameObject.SetActive(false);   
            viewIDs[i] = pool[i].photonView.ViewID;
        }

        photonView.RPC(nameof(RPC_DeactivateAllObjects), RpcTarget.Others, viewIDs);
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

    /// <summary>
    /// viewID 가진 게임 오브젝트를 찾아 활성화 및 위치와 회전값 동기화
    /// </summary>
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

    /// <summary>
    /// viewID 가진 게임 오브젝트를 찾아 비활성화
    /// </summary>
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

    [PunRPC]
    protected void RPC_DeactivateAllObjects(int[] viewIDs)
    {
        foreach (int viewID in viewIDs)
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

}
