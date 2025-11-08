using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 전환 시에도 설정 값을 유지하기 위한 클래스
/// </summary>
public class SettingManager : MonoSingleton<SettingManager>
{
    // BGM, SFX 값은 오디오 믹서가 아닌 아래 값을 수정하면 자동 반영됨
    public float bgmVolume = 0.8f;
    public float sfxVolume = 1f;

    public bool isMicOn = false;
    public float remoteVoiceVolume = 1f;

    private void Start()
    {
        isMicOn = VoiceManager.Instance.recorder.TransmitEnabled;
    }
}