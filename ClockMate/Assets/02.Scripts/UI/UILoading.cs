using Define;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define.Loading;

public class UILoading : UIBase
{
    [SerializeField] private TMP_Text climateTip;
    [SerializeField] private RectTransform characterImgRectTransform;
    [SerializeField] private RectTransform parentLayoutGroupRect;

    [SerializeField] private float _characterMoveDuration = 3f;


    private void Awake()
    {
        UIType = UI.UIType.FullScreen;
    }
    
    public override void Show()
    {
        base.Show();
    }

    /// <summary>
    /// 랜덤 기후위기 정보 팁 보여주기
    /// </summary>
    public void ShowRandomTip(string randomTip)
    {
        if(climateTip != null && randomTip != null)
        {
            climateTip.text = randomTip;
            ForceLayoutRebuild();
        }
    }

    private void ForceLayoutRebuild()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(climateTip.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayoutGroupRect);
    }

    public IEnumerator MoveCharacater(Vector2 startPos, Vector2 endPos)
    {
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