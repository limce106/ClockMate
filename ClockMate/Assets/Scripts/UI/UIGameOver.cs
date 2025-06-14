using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : UIBase
{
    [SerializeField] private Button retryButton;
    private void Awake()
    {
        retryButton.onClick.AddListener(BtnClick);
    }

    private void BtnClick()
    {
        GameManager.Instance.CurrentStage.Reset();
        GameManager.Instance.SetAllCharactersActive(true);
        UIManager.Instance.Close(this);
    }
}
