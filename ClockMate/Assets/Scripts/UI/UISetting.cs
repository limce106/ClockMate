using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��ɵ��� �����ϴ� Ŭ����
/// ����ڰ� ������ ���� SettingsManager�� �����Ͽ� ���� �� ����
/// </summary>
public class UISetting : UIBase
{
    public Button micButton;
    public Slider remoteVoiceVolumeSlider;

    public Sprite micOnSprite;
    public Sprite micOffSprite;

    private AudioSource _remoteAudio;   // ��� �����

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
    /// ���� UI �ʱ�ȭ
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
    /// ����ũ Ŭ�� �� On/Off
    /// </summary>
    public void ToggleMic()
    {
        SettingManager.Instance.isMicOn = !SettingManager.Instance.isMicOn;

        VoiceManager.Instance?.SetMicActive(SettingManager.Instance.isMicOn);
        UpdateMicIcon(SettingManager.Instance.isMicOn);
    }

    /// <summary>
    /// �����̴� ���� �� ��� ���� ũ�� ����
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
