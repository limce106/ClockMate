using Define;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using static Define.Loading;

public class UILoading : UIBase
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text climateTip;
    [SerializeField] private RectTransform characterImgRectTransform;

    [SerializeField] private float _characterMoveDuration = 3f;


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

    public IEnumerator MoveCharacater(Vector2 endPos)
    {
        Vector2 startPos = characterImgRectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < _characterMoveDuration)
        {
            characterImgRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / _characterMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        characterImgRectTransform.anchoredPosition = endPos;

        LoadingManager.Instance.photonView.RPC("NotifyLoadState", RpcTarget.MasterClient,
                    PhotonNetwork.LocalPlayer.ActorNumber, (int)LoadState.Load);
    }
}