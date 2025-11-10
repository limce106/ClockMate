using System.Collections;
using UnityEngine;

public class UISaving : UIBase
{
    public override void Show()
    {
        base.Show();
        StartCoroutine(CloseAfterDelay(3f));
    }
    
    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }
}
