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
    public Image nonSelectableImg;
    public int selectedByActorNumber = -1;
    public string characterName;
} 

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    public static CharacterSelectManager instance;

    public CharacterSlot[] characters;
    public TMP_Text statusText;
    public GameObject gameReady;

    public Image player1CharacterImg;
    public Image player2CharacterImg;

    private int _localActorNumber;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        _localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (var character in characters)
        {
            character.characterButton.onClick.AddListener(() => OnCharacterClicked(character));
        }
    }

    void OnCharacterClicked(CharacterSlot character)
    {
        if(character.selectedByActorNumber == _localActorNumber)
        {
            photonView.RPC("DeselectCharacter", RpcTarget.All, GetCharacterIndex(character), _localActorNumber);
        }
        else if(character.selectedByActorNumber == -1 && !HasPlayerSelected(_localActorNumber))
        {
            photonView.RPC("SelectCharacter", RpcTarget.All, GetCharacterIndex(character), _localActorNumber);
        }
    }

    [PunRPC]
    void SelectCharacter(int index, int actorNumber)
    {
        characters[index].selectedByActorNumber = actorNumber;
        characters[index].nonSelectableImg.gameObject.SetActive(true);

        Sprite characterSprite = Resources.Load<Sprite>("UI/Sprites/Character/" + characters[index].characterName + "_Sticker");
        if (characterSprite == null)
        {
            Debug.LogWarning($"Sprite for {characters[index].characterName} not found in Resources.");
            return;
        }

        if(actorNumber == PhotonNetwork.MasterClient.ActorNumber)
        {
            player1CharacterImg.sprite = characterSprite;
            player1CharacterImg.gameObject.SetActive(true);
        }
        else
        {
            player2CharacterImg.sprite = characterSprite;
            player2CharacterImg.gameObject.SetActive(true);
        }

        UpdateButtonsInteractable();
        UpdateStatusText();
    }

    [PunRPC]
    void DeselectCharacter(int index, int actorNumber)
    {
        characters[index].selectedByActorNumber = -1;
        characters[index].nonSelectableImg.gameObject.SetActive(false);

        if (actorNumber == PhotonNetwork.MasterClient.ActorNumber)
        {
            player1CharacterImg.sprite = null;
            player1CharacterImg.gameObject.SetActive(false);
        }
        else
        {
            player2CharacterImg.sprite = null;
            player2CharacterImg.gameObject.SetActive(false);
        }

        UpdateButtonsInteractable();
        UpdateStatusText();

        RPCManager.Instance.photonView.RPC("ResetAllReadyStates", RpcTarget.All);
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
        bool hasSelected = HasPlayerSelected(_localActorNumber);

        foreach (var c in characters)
        {
            // 아직 선택 안 된 캐릭터
            bool isUnselected = c.selectedByActorNumber == -1;
            // 내가 선택한 캐릭터
            bool isMySelection = c.selectedByActorNumber == _localActorNumber;

            c.characterButton.interactable = !hasSelected || isUnselected || isMySelection;
        }
    }

    void UpdateStatusText()
    {
        bool localSelected = HasPlayerSelected(_localActorNumber);
        bool otherSelected = false;

        foreach(var c in characters)
        {
            if(c.selectedByActorNumber != -1 && c.selectedByActorNumber != _localActorNumber)
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
            statusText.text = "상대방이 캐릭터 선택을 완료했습니다.";
        }
        else if (localSelected && !otherSelected)
        {
            statusText.text = "상대방 캐릭터 선택을 기다리는 중...";
        }
        else
        {
            statusText.text = "모든 플레이어 캐릭터 선택 완료!";
            canAcceptReady = true;
        }

        if(canAcceptReady)
        {
            gameReady.SetActive(true);
        }
        else
        {
            gameReady.SetActive(false);
        }

        RPCManager.Instance.photonView.RPC("SetCanAcceptReady", RpcTarget.All, canAcceptReady);
    }
}
