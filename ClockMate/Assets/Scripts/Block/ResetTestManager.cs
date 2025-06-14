using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetTestManager : MonoSingleton<ResetTestManager>
{
    public Button resetButton;
    private List<ResettableBase> _resettableList = new List<ResettableBase>();
    public Action cbResetFinished; // 초기화 완료 시 호출되는 콜백
    
    public void ResetAllBlocks()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        foreach (ResettableBase resettable in _resettableList)
        {
            if(photonView)
            {
                photonView.RPC("ResetObject", RpcTarget.AllBuffered);
            }
        }
        Debug.Log("초기화 완료");
        cbResetFinished?.Invoke();
        cbResetFinished = null;
    }

    public void AddResettable(ResettableBase resettable)
    {
        _resettableList.Add(resettable);
    }

    public void RemoveAllResettable()
    {
        _resettableList.Clear();
    }
}
