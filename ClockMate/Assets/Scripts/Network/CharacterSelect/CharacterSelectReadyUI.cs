using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CharacterSelectReadyUI : MonoBehaviourPunCallbacks
{
    public GameObject Player1Ready;
    public GameObject Player2Ready;

    void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        var readyDict = RPCManager.GetPlayerReadyStatus();

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = player.Value.ActorNumber;
            bool isMasterClient = player.Value.IsMasterClient;

            bool isReady;
            readyDict.TryGetValue(actorNumber, out isReady);

            UpdateReadyUI(actorNumber, isReady, isMasterClient);
        }
    }

    private void UpdateReadyUI(int actorNumber, bool isReady, bool isMasterClient)
    {
        if(isMasterClient)
        {
            if(Player1Ready)
                Player1Ready.SetActive(isReady);
        }
        else
            if (Player2Ready)
                Player2Ready.SetActive(isReady);
    }
}
