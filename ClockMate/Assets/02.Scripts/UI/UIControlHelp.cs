using UnityEngine;
using UnityEngine.UI;

public class UIControlHelp : UIBase
{
    [SerializeField] private Image img1;
    [SerializeField] private Image img2;
    [SerializeField] private Text txt1;
    [SerializeField] private Text txt2;
    
    public void SetControl(Sprite s1, string t1, Sprite s2, string t2)
    {
        img1.sprite = s1;
        img2.sprite = s2;
        txt1.text = t1;
        txt2.text = t2;
    }
    
    public void SetControl(bool firstOrder, Sprite s, string t)
    {
        if (firstOrder)
        {
            img1.sprite = s;
            txt1.text = t;
        }
        else
        {
            img2.sprite = s;
            txt2.text = t;
        }
    }
}
