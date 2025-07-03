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
    public GameObject remoteSpeakerUI;   // ��밡 ���ϴ� �� ������
    public Image remoteCharacterImg;

    private PhotonVoiceView _remotePhotonVoiceView;  // ��� ����Ŀ

    private const float VoiceDetectionThreshold = 0.1f;

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
        if (remotePlayerName != null)
        {
            _remotePhotonVoiceView = GameObject.FindWithTag(remotePlayerName)?.GetComponent<PhotonVoiceView>();

            if (_remotePhotonVoiceView == null)
            {
                return;
            }
        }

        Sprite characterSprite = Resources.Load<Sprite>("UI/Sprites/" + remotePlayerName + "Icon");
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
        }

        if (remoteCharacterImg.sprite == null)
            return;

        float peakAmp = VoiceManager.Instance.recorder.LevelMeter.CurrentPeakAmp;
        bool isSpeaking = peakAmp >= VoiceDetectionThreshold;

        if (remoteSpeakerUI.activeSelf != isSpeaking)
        {
            remoteSpeakerUI.SetActive(isSpeaking);
        }
    }

    public void OnClick_Setting()
    {
        UISetting uiSetting = UIManager.Instance?.Show<UISetting>("UISetting");
    }
}
