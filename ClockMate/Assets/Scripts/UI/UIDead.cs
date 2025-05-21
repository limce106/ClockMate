using UnityEngine;
using UnityEngine.UI;

public class UIDead : UIBase
{
    [SerializeField] private Text txtMainText;

    public void SetText(string text)
    {
        txtMainText.text = text;
    }

    public void ShowMainText()
    {
        txtMainText.gameObject.SetActive(true);
    }

    public void HideMainText()
    {
        txtMainText.gameObject.SetActive(false);
    }
    
    
}