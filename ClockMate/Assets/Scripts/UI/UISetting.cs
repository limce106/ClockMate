using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 기능들을 정의하는 클래스
/// 사용자가 변경한 값을 SettingsManager에 전달하여 설정 값 갱신
/// </summary>
public class UISetting : UIBase
{
    public Button micButton;
    public Slider remoteVoiceVolumeSlider;

    public Sprite micOnSprite;
    public Sprite micOffSprite;

    private AudioSource _remoteAudio;   // 상대 오디오

    private void Awake()
    {
        string remotePlayerName = GameManager.Instance?.GetRemotePlayerName();
        if (remotePlayerName != null)
        {
            _remoteAudio = GameObject.Find(remotePlayerName)?.GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        InitSettingUI();
    }

    private void OnEnable()
    {
        InitSettingUI();
    }

    /// <summary>
    /// 설정 UI 초기화
    /// </summary>
    private void InitSettingUI()
    {
        UpdateMicIcon(SettingManager.Instance.isMicOn);
        remoteVoiceVolumeSlider.value = SettingManager.Instance.remoteVoiceVolume;
    }

    private void UpdateMicIcon(bool isOn)
    {
        micButton.image.sprite = isOn ? micOnSprite : micOffSprite;
    }

    public void ToggleMic()
    {
        SettingManager.Instance.isMicOn = !SettingManager.Instance.isMicOn;

        VoiceManager.Instance?.SetMicActive(SettingManager.Instance.isMicOn);
        UpdateMicIcon(SettingManager.Instance.isMicOn);
    }

    public void SetRemoteVoiceVolume(float value)
    {
        SettingManager.Instance.remoteVoiceVolume = value;
        _remoteAudio.volume = value;
    }
}
