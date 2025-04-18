using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    public GameObject HourCharacterPrefab;
    public GameObject MilliCharacterPrefab;

    private int myCharacterIndex;

    void Start()
    {
        AssignInitialCharacter();

        RPCManager.Instance.SetCanAcceptReady(true);
        RPCManager.Instance.SetSceneName("Desert");
    }

    void Update()
    {
        
    }

    void AssignInitialCharacter()
    {
        var players = PhotonNetwork.PlayerList;
        System.Array.Sort(players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        myCharacterIndex = System.Array.IndexOf(players, PhotonNetwork.LocalPlayer);
    }

    void OnClick_Reverse()
    {
        photonView.RPC("ReverseCharacters", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ReverseCharacters()
    {
        myCharacterIndex = 1 - myCharacterIndex;

        // 캐릭터 위치 스왑
        Vector3 temp = HourCharacterPrefab.transform.position;
        HourCharacterPrefab.transform.position = MilliCharacterPrefab.transform.position;
        MilliCharacterPrefab.transform.position = temp;
    }
}
