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
            _remoteAudio = GameObject.FindWithTag(remotePlayerName)?.GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        InitSetting();
    }

    private void OnEnable()
    {
        InitSetting();
    }

    /// <summary>
    /// 설정 UI 초기화
    /// </summary>
    private void InitSetting()
    {
        remoteVoiceVolumeSlider.onValueChanged.AddListener((float value) =>
        {
            SetRemoteVoiceVolume(value);
        });

        UpdateMicIcon(SettingManager.Instance.isMicOn);
        remoteVoiceVolumeSlider.value = SettingManager.Instance.remoteVoiceVolume;
    }

    private void UpdateMicIcon(bool isOn)
    {
        micButton.image.sprite = isOn ? micOnSprite : micOffSprite;
    }

    /// <summary>
    /// 마이크 클릭 시 On/Off
    /// </summary>
    public void ToggleMic()
    {
        SettingManager.Instance.isMicOn = !SettingManager.Instance.isMicOn;

        VoiceManager.Instance?.SetMicActive(SettingManager.Instance.isMicOn);
        UpdateMicIcon(SettingManager.Instance.isMicOn);
    }

    /// <summary>
    /// 슬라이더 조절 시 상대 음성 크기 조정
    /// </summary>
    public void SetRemoteVoiceVolume(float value)
    {
        if (_remoteAudio == null)
            return;

        SettingManager.Instance.remoteVoiceVolume = value;
        _remoteAudio.volume = value;
    }

    public void OnClick_Close()
    {
        UIManager.Instance?.Close(this);
    }
}
