using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class UIVoiceChat : MonoBehaviourPun
{
    [SerializeField] private Image _volume;
    private PhotonVoiceView _remotePhotonVoiceView;  // 상대 스피커
    private Speaker _speaker;

    private float sendInterval = 0.1f; // 전송 빈도 조절
    private float nextSendTime = 0f;    // 다음 전송 시간 기록

    private const float HighVolumeThreshold = 0.1f;
    private const float MiddleVolumeThreshold = 0.05f;

    private void OnEnable()
    {
        StartCoroutine(SetRemotePhotonVoiceView());
    }
    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            if (VoiceManager.Instance.recorder != null && VoiceManager.Instance.recorder.LevelMeter != null)
            {
                // 설정된 전송 간격이 되었는지 확인
                if (Time.time >= nextSendTime)
                {
                    // 자신의 마이크의 볼륨 크기
                    float averageAmplitude = VoiceManager.Instance.recorder.LevelMeter.CurrentAvgAmp;
                    Debug.Log(VoiceManager.Instance.recorder.LevelMeter.CurrentAvgAmp);

                    // 측정된 볼륨 값을 다른 모든 플레이어에게 전송
                    photonView.RPC(nameof(ReceiveVoiceLevel), RpcTarget.Others, averageAmplitude);

                    // 다음 전송 시간 갱신
                    nextSendTime = Time.time + sendInterval;
                }
            }
        }
    }

    private IEnumerator SetRemotePhotonVoiceView()
    {
        string remotePlayerName = GameManager.Instance?.GetRemotePlayerName();
        if (!string.IsNullOrEmpty(remotePlayerName))
        {
            _remotePhotonVoiceView = GameObject.FindWithTag(remotePlayerName)?.GetComponentInParent<PhotonVoiceView>();
            if (_remotePhotonVoiceView == null)
                yield return null;

            _speaker = _remotePhotonVoiceView.SpeakerInUse;
        }
    }

    [PunRPC]
    public void ReceiveVoiceLevel(float level, PhotonMessageInfo info)
    {
        // 발신자가 로컬 플레이어와 다르다면
        if (info.Sender.ActorNumber != photonView.Owner.ActorNumber)
        {
            return;
        }

        Sprite volumeSprite = null;
        if (level >= HighVolumeThreshold)
        {
            volumeSprite = Resources.Load<Sprite>("UI/Sprites/Mic/Mic_Volume_Three");
        }
        else if(level >= MiddleVolumeThreshold)
        {
            volumeSprite = Resources.Load<Sprite>("UI/Sprites/Mic/Mic_Volume_Two");
        }
        else
        {
            volumeSprite = Resources.Load<Sprite>("UI/Sprites/Mic/Mic_Volume_One");
        }

        _volume.sprite = volumeSprite;
    }
}
