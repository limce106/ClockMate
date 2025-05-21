using Define;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : UIBase
{
    [SerializeField] private Text txtProgressPercent; 
    
    private void Awake()
    {
        UIType = UI.UIType.FullScreen;
    }
    
    public override void Show()
    {
        base.Show();
        UpdateLoadingProgress(0f);
    }
    
    /// <summary>
    /// 로딩 진행도 갱신 (0~1 사이 값)
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        if (txtProgressPercent != null)
            txtProgressPercent.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }
}