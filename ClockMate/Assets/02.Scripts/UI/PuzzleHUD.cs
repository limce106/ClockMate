using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PuzzleHUD : UIBase
{
    public GameObject remoteSpeakerUI;   // 상대가 말하는 중 아이콘
    public Image remoteCharacterImg;
    [SerializeField] private UIQuest _uiQuest; 

    private PhotonVoiceView _remotePhotonVoiceView;  // 상대 스피커

    void Start()
    {
        remoteSpeakerUI.SetActive(false);
    }

    public override void Show()
    {
        if (SceneManager.GetActiveScene().name == "ClockTower")
        {
            HideQuest();
        }
        else
        {
            ShowAndUpdateQuest();
        }
    }

    void Update()
    {
        ToggleQuest();
        UpdateRemoteSpeaking();
    }

    private void InitRemoteSpeaker()
    {
        string remotePlayerName = GameManager.Instance?.GetRemotePlayerName();
        if (!string.IsNullOrEmpty(remotePlayerName))
        {
            _remotePhotonVoiceView = GameObject.FindWithTag(remotePlayerName)?.GetComponentInParent<PhotonVoiceView>();
        }

        Sprite characterSprite = Resources.Load<Sprite>("UI/Sprites/Mic/Mic_Using_" + remotePlayerName);
        if (characterSprite == null)
        {
            Debug.LogWarning($"Sprite for {remotePlayerName} not found in Resources.");
            return;
        }

        remoteCharacterImg.sprite = characterSprite;
    }

    private void UpdateRemoteSpeaking()
    {
        if (_remotePhotonVoiceView == null)
        {
            InitRemoteSpeaker();

            if (_remotePhotonVoiceView == null)
                return;
        }

        if (remoteCharacterImg.sprite == null)
            return;

        bool isSpeaking = _remotePhotonVoiceView != null && _remotePhotonVoiceView.IsSpeaking;

        if (remoteSpeakerUI.activeSelf != isSpeaking)
        {
            remoteSpeakerUI.SetActive(isSpeaking);
        }
    }

    public void ShowAndUpdateQuest()
    {
        _uiQuest.gameObject.SetActive(true);
        _uiQuest.UpdateQuest();
    }

    public void HideQuest()
    {
        _uiQuest.gameObject.SetActive(false);
    }

    private void ToggleQuest()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            bool active = _uiQuest.gameObject.activeSelf;
            _uiQuest.gameObject.SetActive(!active);
        }
    }
}
