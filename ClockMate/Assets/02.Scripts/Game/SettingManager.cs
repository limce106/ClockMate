using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 전환 시에도 설정 값을 유지하기 위한 클래스
/// </summary>
public class SettingManager : MonoSingleton<SettingManager>
{
    public bool isMicOn = true;
    public float remoteVoiceVolume = 1f;
}
