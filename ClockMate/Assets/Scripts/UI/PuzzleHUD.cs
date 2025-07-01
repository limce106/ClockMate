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
    public GameObject remoteSpeakerUI;   // ��밡 ���ϴ� �� ������
    public Image remoteCharacterImg;

    private PhotonVoiceView _remoteSpeaker;  // ��� ����Ŀ

    private void Awake()
    {
        UIType = UI.UIType.FullScreen;
    }

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
            _remoteSpeaker = GameObject.Find(remotePlayerName)?.GetComponent<PhotonVoiceView>();
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
        if (_remoteSpeaker == null)
            return;

        bool speaking = _remoteSpeaker.IsSpeaking;

        if (remoteSpeakerUI.activeSelf != speaking)
        {
            remoteSpeakerUI.SetActive(speaking);
        }
    }
}
