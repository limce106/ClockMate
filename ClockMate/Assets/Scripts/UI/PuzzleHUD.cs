using Define;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Voice.Unity.Demos.DemoVoiceUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleHUD : UIBase
{
    public GameObject remoteSpeakerUI;   // 상대가 말하는 중 아이콘
    public Image remoteCharacterImg;

    private PhotonVoiceView _remotePhotonVoiceView;  // 상대 스피커

    void Start()
    {
        remoteSpeakerUI.SetActive(false);
        InitRemoteSpeaker();
    }

    void Update()
    {
        UpdateRemoteSpeaking();
    }

    private void InitRemoteSpeaker()
    {
        string remotePlayerName = GameManager.Instance?.GetRemotePlayerName();
        if (remotePlayerName != null)
        {
            _remotePhotonVoiceView = GameObject.Find(remotePlayerName)?.GetComponent<PhotonVoiceView>();
        }

        Sprite characterSprite = Resources.Load<Sprite>("UI/Sprites/Character/" + remotePlayerName + "_Sticker");
        if (characterSprite == null)
        {
            Debug.LogWarning($"Sprite for {remotePlayerName} not found in Resources.");
            return;
        }

        remoteCharacterImg.sprite = characterSprite;
    }

    private void UpdateRemoteSpeaking()
    {
        if (_remotePhotonVoiceView == null)
            return;

        bool speaking = _remotePhotonVoiceView.IsSpeaking;

        if (remoteSpeakerUI.activeSelf != speaking)
        {
            remoteSpeakerUI.SetActive(speaking);
        }
    }
}
