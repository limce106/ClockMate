using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CharacterSlot
{
    public Button characterButton;
    public TMP_Text playerText;
    public int selectedByActorNumber = -1;
}

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    public static CharacterSelectManager instance;

    public CharacterSlot[] characters;
    public TMP_Text statusText;

    private int localActorNumber;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (var character in characters)
        {
            character.characterButton.onClick.AddListener(() => OnCharacterClicked(character));
        }

        RPCManager.Instance.photonView.RPC("SetSceneName", RpcTarget.All, "Desert");
    }

    void OnCharacterClicked(CharacterSlot character)
    {
        if(character.selectedByActorNumber == localActorNumber)
        {
            photonView.RPC("DeselectCharacter", RpcTarget.All, GetCharacterIndex(character));
        }
        else if(character.selectedByActorNumber == -1 && !HasPlayerSelected(localActorNumber))
        {
            photonView.RPC("SelectCharacter", RpcTarget.All, GetCharacterIndex(character), localActorNumber);
        }
    }

    [PunRPC]
    void SelectCharacter(int index, int actorNumber)
    {
        characters[index].selectedByActorNumber = actorNumber;
        characters[index].playerText.gameObject.SetActive(true);
        characters[index].playerText.text = (actorNumber == PhotonNetwork.MasterClient.ActorNumber) ? "Player1" : "Player2";

        UpdateButtonsInteractable();
        UpdateStatusText();
    }

    [PunRPC]
    void DeselectCharacter(int index)
    {
        characters[index].selectedByActorNumber = -1;
        characters[index].playerText.gameObject.SetActive(false);

        UpdateButtonsInteractable();
        UpdateStatusText();
    }

    public int GetCharacterIndex(CharacterSlot character)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == character)
                return i;
        }
        return -1;
    }

    
    bool HasPlayerSelected(int actorNumber)
    {
        // 이미 캐릭터를 선택했는지
        foreach (var c in characters)
        {
            if (c.selectedByActorNumber == actorNumber)
                return true;
        }
        return false;
    }

    void UpdateButtonsInteractable()
    {
        // 내가 아무 캐릭터도 선택하지 않음
        bool hasSelected = HasPlayerSelected(localActorNumber);

        foreach (var c in characters)
        {
            // 아직 선택 안 된 캐릭터
            bool isUnselected = c.selectedByActorNumber == -1;
            // 내가 선택한 캐릭터
            bool isMySelection = c.selectedByActorNumber == localActorNumber;

            c.characterButton.interactable = !hasSelected || isUnselected || isMySelection;
        }
    }

    void UpdateStatusText()
    {
        bool localSelected = HasPlayerSelected(localActorNumber);
        bool otherSelected = false;

        foreach(var c in characters)
        {
            if(c.selectedByActorNumber != -1 && c.selectedByActorNumber != localActorNumber)
            {
                otherSelected = true;
                break;
            }
        }

        bool canAcceptReady = false;

        if(!localSelected && !otherSelected)
        {
            statusText.text = "어떤 캐릭터를 선택하시겠어요?";
        }
        else if (!localSelected && otherSelected)
        {
            statusText.text = "상대방이 캐릭터 선택 완료했습니다.";
        }
        else if (localSelected && !otherSelected)
        {
            statusText.text = "상대방 캐릭터 선택을 기다리는 중...";
        }
        else
        {
            statusText.text = "캐릭터 선택 완료! E키를 눌러 준비하세요.";
            canAcceptReady = true;
        }

        RPCManager.Instance.photonView.RPC("SetCanAcceptReady", RpcTarget.All, canAcceptReady);
    }
}
