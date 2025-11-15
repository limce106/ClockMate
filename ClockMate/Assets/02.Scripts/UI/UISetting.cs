using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public TMP_Text bgmValue;
    public TMP_Text sfxValue;
    public TMP_Text remoteVoiceValue;

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

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// 설정 UI 초기화
    /// </summary>
    private void InitSetting()
    {
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSettingSfxVolume);
        remoteVoiceVolumeSlider.onValueChanged.AddListener(SetRemoteVoiceVolume);

        UpdateMicIcon(SettingManager.Instance.isMicOn);

        bgmVolumeSlider.value = SettingManager.Instance.bgmVolume;
        sfxVolumeSlider.value = SettingManager.Instance.sfxVolume;
        remoteVoiceVolumeSlider.value = SettingManager.Instance.remoteVoiceVolume;
    }

    private void UpdateSettingSfxVolume(float value)
    {
        SettingManager.Instance.sfxVolume = value;

        int textValue = (int)(value * 100);
        sfxValue.text = textValue.ToString();
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

        int textValue = (int)(value * 100);
        bgmValue.text = textValue.ToString();
    }

    /// <summary>
    /// 슬라이더 조절 시 상대 음성 볼륨 조정
    /// </summary>
    public void SetRemoteVoiceVolume(float value)
    {
        SettingManager.Instance.remoteVoiceVolume = value;

        int textValue = (int)(value * 100);
        remoteVoiceValue.text = textValue.ToString();

        if (_remoteAudio != null)
            _remoteAudio.volume = value;
    }

    public void OnClick_Continue()
    {
        Cursor.lockState = CursorLockMode.Confined;
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);

        UIManager.Instance?.Close(this);
    }

    public void OnClick_Exit()
    {
        Cursor.lockState = CursorLockMode.Confined;
        SoundManager.Instance.PlaySfx(key: "ui_click", pos: null, volume: 0.7f);

        PhotonNetwork.LeaveRoom();
    }
}
