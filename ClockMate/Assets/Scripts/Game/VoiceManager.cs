using Photon.Pun;
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

    private void Start()
    {
        InitVoiceClient();
    }

    public void InitVoiceClient()
    {
        if (voiceClient.Client == null || !voiceClient.Client.IsConnected)
        {
            voiceClient.ConnectUsingSettings();
        }

        if (PhotonNetwork.InRoom)
        {
            recorder.TransmitEnabled = true;
        }

        Debug.Log("PunVoiceClient");
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
