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
        if (NetworkManager.Instance.IsInRoomAndReady() && PhotonNetwork.IsMasterClient)
        {
            foreach (ResettableBase resettable in _resettableList)
            {
                resettable.photonView.RPC(nameof(resettable.RPC_ResetObject), RpcTarget.All);
            }
        }
        else
        {
            foreach (ResettableBase resettable in _resettableList)
            {
                resettable.ResetObject();
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
