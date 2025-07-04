using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using static Define.Character;

public class CharacterSelectFinalizer : MonoBehaviour
{
    [SerializeField]
    private CharacterSelectManager _characterSelectManager;

    public GameObject Player1Ready;
    public GameObject Player2Ready;

    private bool _isLoadingStarted = false;

    private void Start()
    {
        RPCManager.OnSyncedAllReadyAction = () =>
        {
            StartCoroutine(HandleAllReadySequence());
        };
    }

    private IEnumerator HandleAllReadySequence()
    {
        SaveSelectedCharacter();
        _isLoadingStarted = true;

        Player1Ready?.SetActive(true);
        Player2Ready?.SetActive(true);

        yield return null;

        GameManager.Instance?.CreateNewSaveData();
        LoadingManager.Instance?.StartSyncedLoading(GameManager.Instance?.CurrentStage.Map.ToString());
    }

    void Update()
    {
        if (_isLoadingStarted)
            return;

        if (!PhotonNetwork.InRoom)
            return;

        UpdateReadyUIForAllPlayers();
    }

    void UpdateReadyUIForAllPlayers()
    {
        var readyDict = RPCManager.GetPlayerReadyStatus();

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            int actorNumber = player.Value.ActorNumber;
            bool isMasterClient = player.Value.IsMasterClient;

            bool isReady = false;
            readyDict.TryGetValue(actorNumber, out isReady);
            UpdateReadyUI(actorNumber, isReady, isMasterClient);
        }
    }

    private void UpdateReadyUI(int actorNumber, bool isReady, bool isMasterClient)
    {
        if (isMasterClient)
        {
            Player1Ready?.SetActive(isReady);
            Debug.Log("Player1: " + isReady);
        }
        else
        {
            Player2Ready?.SetActive(isReady);
            Debug.Log("Player2: " + isReady);
        }
    }

    private void SaveSelectedCharacter()
    {
        if (!_characterSelectManager)
            return;

        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (var slot in _characterSelectManager.characters)
        {
            if (slot.selectedByActorNumber != localActorNumber)
                continue;

            int index = _characterSelectManager.GetCharacterIndex(slot);
            CharacterName character = (CharacterName)index;

            GameManager.Instance.SetSelectedCharacter(character);
            Debug.Log($"[CharacterSelectReadyUI] 내 선택 캐릭터 저장됨: {character}");
            break;
        }
    }
}
