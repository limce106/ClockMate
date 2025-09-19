using Define;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : UIBase
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text climateTip;

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
        if (progressSlider != null)
            progressSlider.value = progress;
    }

    /// <summary>
    /// 랜덤 기후위기 정보 팁 보여주기
    /// </summary>
    public void ShowRandomTip(string randomTip)
    {
        if(climateTip != null && randomTip != null)
            climateTip.text = randomTip;
    }
}