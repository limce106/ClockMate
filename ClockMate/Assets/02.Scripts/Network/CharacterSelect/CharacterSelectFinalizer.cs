using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class CharacterSelectFinalizer : MonoBehaviourPun
{
    [SerializeField]
    private CharacterSelectManager _characterSelectManager;

    public GameObject Player1Ready;
    public GameObject Player2Ready;

    private bool _isLoadingStarted = false;
    private bool _isCutsceneFinished = false;   // 컷씬 끝났는지 확인

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

        yield return new WaitForSeconds(1f);

        GameManager.Instance?.CreateNewSaveData();
        photonView.RPC(nameof(RPC_KronosAdvent), RpcTarget.All);
        LoadingManager.Instance?.ShowLoadingUI();

        yield return new WaitUntil(() => _isCutsceneFinished);

        LoadingManager.Instance?.StartSyncedLoading(GameManager.Instance?.CurrentStage.Map.ToString());
    }

    [PunRPC]
    private void RPC_KronosAdvent()
    {
        CutsceneSyncManager.Instance.PlayForAll(
            "KronosAdvent",
            0f,
            () => 
            {
                photonView.RPC(nameof(RPC_NotifyCutsceneFinished), RpcTarget.All);
            }
        );
    }

    [PunRPC]
    private void RPC_NotifyCutsceneFinished()
    {
        _isCutsceneFinished = true;
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
