using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : UIBase
{
    [SerializeField] private Button retryButton;
    private void Awake()
    {
        retryButton.onClick.AddListener(BtnClick);
        GameManager.Instance.SetLocalCharacterInput(false);
    }

    private void BtnClick()
    {
        GameManager.Instance.CurrentStage.Reset();
        GameManager.Instance.Characters[GameManager.Instance.SelectedCharacter].photonView.RPC("SetCharacterActive", RpcTarget.All, true);
        GameManager.Instance.SetLocalCharacterInput(true);
        UIManager.Instance.Close(this);
    }
}
