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
        }
        if (recorder == null)
        {
            recorder = GetComponent<Recorder>();
        }

        PunVoiceClient.Instance.PrimaryRecorder = recorder;
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
