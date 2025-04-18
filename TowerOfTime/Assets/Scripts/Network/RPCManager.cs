using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RPCManager : MonoBehaviourPunCallbacks
{
    private static RPCManager instance;
    public static RPCManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<RPCManager>();
                if (obj != null)
                    instance = obj;
            }
            return instance;
        }
    }

    private PhotonView PV;
    private static HashSet<int> readPlayers = new HashSet<int>(2);
    private bool canAcceptReady = false;
    private string sceneName = "";

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom || !canAcceptReady || !PV)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PV.RPC("MarkReady", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    public void SetCanAcceptReady(bool value)
    {
        canAcceptReady = value;
    }

    [PunRPC]
    public void SetSceneName(string name)
    {
        sceneName = name;
    }

    [PunRPC]
    void MarkReady(int actorNumber)
    {
        if (readPlayers.Contains(actorNumber))
            return;

        readPlayers.Add(actorNumber);

        if (readPlayers.Count == PhotonNetwork.CurrentRoom.MaxPlayers && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
            readPlayers.Clear();
        }
    }
}
