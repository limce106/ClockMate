using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Define;

public enum MicButtonType
{
    Hour,
    Milli
}

public class UIPlayerMic : MonoBehaviourPun
{
    private Button micButton;
    public Sprite micOnSprite;
    public Sprite micOffSprite;

    private bool isMicOn = true;
    public MicButtonType micButtonType;

    void Start()
    {
        micButton = GetComponent<Button>();

        // 실제 게임 실행 시 코드 바꾸기
        //micButton.interactable = (micButtonType == MicButtonType.Hour && GameManager.Instance?.SelectedCharacter == Character.CharacterName.Hour)
        //                        || (micButtonType == MicButtonType.Milli && GameManager.Instance?.SelectedCharacter == Character.CharacterName.Milli);

        // 테스트용
        if(PhotonNetwork.IsMasterClient)
        {
            micButton.interactable = micButton.gameObject.name.Contains("Hour");
            Debug.Log("Hour Mic On");
        }
        else
        {
            micButton.interactable = micButton.gameObject.name.Contains("Milli");
            Debug.Log("Milli Mic On");
        }
        //

        photonView.RPC(nameof(SetRemoteMicIcon), RpcTarget.All, isMicOn);
    }

    private void UpdateMicIcon(bool isOn)
    {
        micButton.image.sprite = isOn ? micOnSprite : micOffSprite;
    }

    [PunRPC]
    void SetRemoteMicIcon(bool isOn)
    {
        UpdateMicIcon(isOn);
    }

    public void ToggleMic()
    {
        isMicOn = !isMicOn;

        VoiceManager.Instance?.SetMicActive(isMicOn);
        UpdateMicIcon(isMicOn);

        photonView.RPC(nameof(SetRemoteMicIcon), RpcTarget.Others, isMicOn);
    }
}
