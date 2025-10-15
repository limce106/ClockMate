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
    private HashSet<int> actorNums = new HashSet<int>();

    private IEnumerator HandleAllReadySequence()
    {
        _characterSelectManager.statusText.text = "잠시 후 <color=#FFD13A>아워와 밀리의 모험</color>이 시작됩니다!";
        SaveSelectedCharacter();
        _isLoadingStarted = true;

        yield return new WaitForSeconds(1.5f);

        GameManager.Instance?.CreateNewSaveData();
        if (PhotonNetwork.IsMasterClient)
        {
            CutsceneSyncManager.Instance.PlayForAll(
                "KronosAdvent",
                0f,
                () => 
                {
                    LoadingManager.Instance.photonView.RPC("RPC_LoadScene", RpcTarget.All, GameManager.Instance?.CurrentStage.Map.ToString());
                }
            );
        }
        LoadingManager.Instance.ShowLoadingUI();
    }

    void Update()
    {
        if (_isLoadingStarted)
            return;

        if (!PhotonNetwork.InRoom)
            return;

        if (_characterSelectManager.actorNumcharacter.Count != 2)
            return;

        if(Input.GetKeyDown(KeyCode.E))
        {
            photonView.RPC(nameof(RPC_UpdateReadyUI), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void RPC_UpdateReadyUI(int actorNum)
    {
        var characterSlot = _characterSelectManager.actorNumcharacter[actorNum];

        bool isActive = !characterSlot.ready.activeSelf;
        characterSlot.ready.SetActive(isActive);

        if(isActive)
        {
            actorNums.Add(actorNum);
        }
        else
        {
            actorNums.Remove(actorNum);
        }

        if(actorNums.Count == 2)
            StartCoroutine(HandleAllReadySequence());
    }

    private void SaveSelectedCharacter()
    {
        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        var slot = _characterSelectManager.actorNumcharacter[localActorNumber];

        int slotIndex = _characterSelectManager.GetCharacterIndex(slot);
        CharacterName character = (CharacterName)slotIndex;

        GameManager.Instance.SetSelectedCharacter(character);
        Debug.Log($"[CharacterSelectReadyUI] 내 선택 캐릭터 저장됨: {character}");
    }
}
