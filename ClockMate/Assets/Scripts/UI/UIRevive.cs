using UnityEngine;
using UnityEngine.UI;
using static Define.Character;

public class UIRevive : UIBase
{
    [SerializeField] private Text text;
    [SerializeField] private Image progress;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image inputKeyImage;

    public void SetUI(bool isDeadPlayer, CharacterName deadCharacter)
    {
        SetProgress(0);
        
        if (isDeadPlayer)
        {
            text.text = "대기중";
            inputKeyImage.enabled = false;
        }
        else
        {
            text.text = "살려주기";
            inputKeyImage.enabled = true;
        }
        
        string imgName = deadCharacter == CharacterName.Hour ? "HourIcon" : "MilliIcon";
        characterImage.sprite = Resources.Load<Sprite>("UI/Sprites/" + imgName);
    }

    public void SetProgress(float amount)
    {
        this.progress.fillAmount = amount;
    }
}
