using Define;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Voice.Unity.Demos.DemoVoiceUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class PuzzleHUD : UIBase
{
    public GameObject remoteSpeakerUI;   // 상대가 말하는 중 아이콘
    public Image remoteCharacterImg;

    private PhotonVoiceView _remotePhotonVoiceView;  // 상대 스피커

    void Start()
    {
        remoteSpeakerUI.SetActive(false);
    }

    void Update()
    {
        UpdateRemoteSpeaking();
    }

    private void InitRemoteSpeaker()
    {
        string remotePlayerName = GameManager.Instance?.GetRemotePlayerName();
        if (!string.IsNullOrEmpty(remotePlayerName))
        {
            _remotePhotonVoiceView = GameObject.FindWithTag(remotePlayerName)?.GetComponent<PhotonVoiceView>();
        }

        Sprite characterSprite = Resources.Load<Sprite>("UI/Sprites/Character/" + remotePlayerName + "Icon");
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
        {
            InitRemoteSpeaker();

            if (_remotePhotonVoiceView == null)
                return;
        }

        if (remoteCharacterImg.sprite == null)
            return;

        bool isSpeaking = _remotePhotonVoiceView != null && _remotePhotonVoiceView.IsSpeaking;

        if (remoteSpeakerUI.activeSelf != isSpeaking)
        {
            remoteSpeakerUI.SetActive(isSpeaking);
        }
    }

    public void OnClick_Setting()
    {
        UISetting uiSetting = UIManager.Instance?.Show<UISetting>("UISetting");
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
}
