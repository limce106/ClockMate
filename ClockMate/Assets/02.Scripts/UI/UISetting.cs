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
    [Header("UI")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
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
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        remoteVoiceVolumeSlider.onValueChanged.AddListener(SetRemoteVoiceVolume);

        if (VoiceManager.Instance != null)
        {
            SettingManager.Instance.isMicOn = VoiceManager.Instance.recorder.TransmitEnabled;
        }
        UpdateMicIcon(SettingManager.Instance.isMicOn);

        if(SettingManager.Instance == null)
        {
            Debug.Log("null");
        }
        else
        {
            Debug.Log("not null");

        }

        bgmVolumeSlider.value = SettingManager.Instance.bgmVolume;
        sfxVolumeSlider.value = SettingManager.Instance.sfxVolume;
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

        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }

    /// <summary>
    /// 슬라이더 조절 시 BGM 볼륨 조정
    /// </summary>
    public void SetBgmVolume(float value)
    {
        SoundManager.Instance.SetBgmVolume(value);
    }

    /// <summary>
    /// 슬라이더 조절 시 SFX 볼륨 조정
    /// </summary>
    public void SetSfxVolume(float value)
    {
        SoundManager.Instance.SetSfxVolume(value);
    }

    /// <summary>
    /// 슬라이더 조절 시 상대 음성 볼륨 조정
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
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);
    }
}
