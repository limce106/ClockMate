using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ��ȯ �ÿ��� ���� ���� �����ϱ� ���� Ŭ����
/// </summary>
public class SettingManager : MonoSingleton<SettingManager>
{
    public bool isMicOn = true;
    public float remoteVoiceVolume = 1f;
}
