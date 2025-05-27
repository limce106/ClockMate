using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using static Define.Character;

public class CharacterSelectFinalizer : MonoBehaviour
{
    [SerializeField]
    private CharacterSelectManager characterSelectManager;

    public GameObject Player1Ready;
    public GameObject Player2Ready;

    private bool hasSavedCharacters = false;

    void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        UpdateReadyUIForAllPlayers();

        if (AreAllPlayersReady() && !hasSavedCharacters)
        {
            SaveSelectedCharacter();
            hasSavedCharacters = true;
        }
    }

    void UpdateReadyUIForAllPlayers()
    {
        var readyDict = RPCManager.GetPlayerReadyStatus();

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = player.Value.ActorNumber;
            bool isMasterClient = player.Value.IsMasterClient;

            bool isReady = readyDict.TryGetValue(actorNumber, out isReady) && isReady;
            UpdateReadyUI(actorNumber, isReady, isMasterClient);
        }
    }

    private void UpdateReadyUI(int actorNumber, bool isReady, bool isMasterClient)
    {
        if (isMasterClient)
        {
            Player1Ready?.SetActive(isReady);
        }
        else
        {
            Player2Ready?.SetActive(isReady);
        }
    }

    private bool AreAllPlayersReady()
    {
        var readyDict = RPCManager.GetPlayerReadyStatus();

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = player.Value.ActorNumber;

            if (!readyDict.TryGetValue(actorNumber, out bool isReady) || !isReady)
                return false;
        }
        return true;
    }

    private void SaveSelectedCharacter()
    {
        if (!characterSelectManager)
            return;

        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (var slot in characterSelectManager.characters)
        {
            if (slot.selectedByActorNumber != localActorNumber)
                continue;

            int index = characterSelectManager.GetCharacterIndex(slot);
            CharacterName character = (CharacterName)index;

            GameManager.Instance.SetSelectedCharacter(character);
            Debug.Log($"[CharacterSelectReadyUI] 내 선택 캐릭터 저장됨: {character}");
            break;
        }
    }
}
