using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance;

    public Recorder recorder;
    public PunVoiceClient voiceClient;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (voiceClient == null)
        {
            voiceClient = GetComponent<PunVoiceClient>();
            GetComponent<VoiceFollowClient>().enabled = false;


        }
        if (recorder == null)
        {
            recorder = GetComponent<Recorder>();
            SetMicActive(false);
        }
    }

    public void ConnectVoice(AppSettings appSettings)
    {
        if(voiceClient != null)
        {
            voiceClient.ConnectUsingSettings(appSettings);
        }
        else
        {
            Debug.LogError("PunVoiceClient�� �������� �ʰų� �̹� ����Ǿ� �־� Voice ������ ������ �� �����ϴ�.");
        }
    }

    public void SetMicActive(bool isActive)
    {
        recorder.TransmitEnabled = isActive;
    }

    public bool IsMicActive()
    {
        return recorder.TransmitEnabled;
    }
}
