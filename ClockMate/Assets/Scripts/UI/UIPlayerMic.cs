using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIPlayerMic : MonoBehaviourPun
{
    private Button micButton;
    public Sprite micOnSprite;
    public Sprite micOffSprite;

    private bool isMicOn = true;

    void Start()
    {
        micButton = GetComponent<Button>();

        micButton.interactable = photonView.IsMine;
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

        photonView.RPC(nameof(SetRemoteMicIcon), RpcTarget.Others);
    }
}
