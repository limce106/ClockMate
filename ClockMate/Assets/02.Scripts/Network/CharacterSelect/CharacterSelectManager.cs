using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

public class CharacterSelectManager : MonoBehaviourPun
{
    [System.Serializable]
    public class CharacterSlot
    {
        public Image pageImg;
        public Image controlImg;
        public TMP_Text controller;
        public Button characterButton;
        public Image cancelImg;
        public GameObject ready;
        public int selectedByActorNumber = -1;
        public string pageLR;
    }

    public static CharacterSelectManager Instance;

    [SerializeField] private CharacterSlot[] characters;
    public TMP_Text statusText;

    public Dictionary<int, CharacterSlot> actorNumcharacter { private set; get; } = new Dictionary<int, CharacterSlot>();
    private int _localActorNumber;


    private void Awake()
    {
        Instance = this;

        SoundManager.Instance.StopAll(SoundType.BGM);

        if (PhotonNetwork.IsMasterClient)
        {
            CutsceneSyncManager.Instance.PlayForAll(
                "Intro",
                0f,
                () => { }
            );
        }
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
        if (character.selectedByActorNumber == _localActorNumber)
        {
            photonView.RPC("DeselectCharacter", RpcTarget.All, GetCharacterIndex(character), _localActorNumber);
        }
        else if (character.selectedByActorNumber == -1 && !HasPlayerSelected(_localActorNumber))
        {
            photonView.RPC("SelectCharacter", RpcTarget.All, GetCharacterIndex(character), _localActorNumber);
        }
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
            {
                return true;
            }
        }
        return false;
    }

    [PunRPC]
    void SelectCharacter(int slotIndex, int actorNumber)
    {
        string owner = (_localActorNumber == actorNumber) ? "Local" : "Remote";

        Sprite pageSprite = Resources.Load<Sprite>("UI/Sprites/" + owner + "_Select_" + characters[slotIndex].pageLR);
        characters[slotIndex].pageImg.GetComponent<Image>().sprite = pageSprite;
        characters[slotIndex].pageImg.gameObject.SetActive(true);

        characters[slotIndex].controller.text = (owner == "Local") ? "나" : "상대";

        Sprite controlSprite = Resources.Load<Sprite>("UI/Sprites/" + owner + "_Control");
        characters[slotIndex].controlImg.GetComponent<Image>().sprite = controlSprite;
        characters[slotIndex].controlImg.gameObject.SetActive(true);

        characters[slotIndex].selectedByActorNumber = actorNumber;

        actorNumcharacter.Add(actorNumber, characters[slotIndex]);

        UpdateButtonsInteractable();
        UpdateStatusText();
    }

    [PunRPC]
    void DeselectCharacter(int slotIndex, int actorNumber)
    {
        characters[slotIndex].pageImg.gameObject.SetActive(false);
        characters[slotIndex].controlImg.gameObject.SetActive(false);
        characters[slotIndex].selectedByActorNumber = -1;

        actorNumcharacter.Remove(actorNumber);

        UpdateButtonsInteractable();
        UpdateStatusText();

        RPCManager.Instance.photonView.RPC("ResetAllReadyStates", RpcTarget.All);
    }

    void UpdateButtonsInteractable()
    {
        // 내가 아무 캐릭터도 선택하지 않음
        bool hasSelected = HasPlayerSelected(_localActorNumber);

        foreach (var character in characters)
        {
            // 아직 선택 안 된 캐릭터
            bool isUnselected = character.selectedByActorNumber == -1;
            // 내가 선택한 캐릭터
            bool isMySelection = character.selectedByActorNumber == _localActorNumber;
            // 다른 플레이어가 선택한 캐릭터
            bool isSelectedByOther = character.selectedByActorNumber != -1 && !isMySelection;

            if (isSelectedByOther)
            {
                // 상대가 선택한 슬롯을 항상 비활성화
                character.characterButton.interactable = false;
            }
            else if (hasSelected)
            {
                // 내가 선택한 상태면 내 슬롯만 활성화
                character.characterButton.interactable = isMySelection;
            }
            else
            {
                // 내가 선택 전이면 빈 슬롯만 활성화
                character.characterButton.interactable = isUnselected;
            }
        }
    }

    void UpdateStatusText()
    {
        bool localSelected = HasPlayerSelected(_localActorNumber);
        bool otherSelected = false;

        foreach (var c in characters)
        {
            if (c.selectedByActorNumber != -1 && c.selectedByActorNumber != _localActorNumber)
            {
                otherSelected = true;
                break;
            }
        }

        bool canAcceptReady = false;

        if (!localSelected && !otherSelected)
        {
            statusText.text = "<color=#FFD13A>원하는 캐릭터</color>를 선택해주세요.";
        }
        else if (!localSelected && otherSelected)
        {
            statusText.text = "상대방이 <color=#FFD13A>캐릭터 선택</color>을 완료했습니다.";
        }
        else if (localSelected && !otherSelected)
        {
            statusText.text = "상대방 <color=#FFD13A>캐릭터 선택</color>을 기다리는 중...";
        }
        else
        {
            statusText.text = "E키를 눌러 게임을 준비할 수 있어요";
            canAcceptReady = true;
        }

        RPCManager.Instance.photonView.RPC("SetCanAcceptReady", RpcTarget.All, canAcceptReady);
    }

    public void PlayUIClickSFX()
    {
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
}
