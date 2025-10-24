using System;
using System.Collections.Generic;
using Define;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class IAClockworkGear : MonoBehaviourPun, IInteractable
{

    public bool CanInteract(CharacterBase character)
    {
        return !CutsceneSyncManager.Instance.IsBusy;
    }

    public void OnInteractAvailable()
    {
    }

    public void OnInteractUnavailable()
    {
    }

    public bool Interact(CharacterBase character)
    {
        // 컷신이 이미 시작되었다면 중복 실행 방지
        if (CutsceneSyncManager.Instance.IsBusy)
        {
            return false;
        }

        photonView.RPC(nameof(RequestPlayCutscene), RpcTarget.MasterClient);
        
        return true;
    }

    [PunRPC]
    private void RequestPlayCutscene()
    {
        if (!PhotonNetwork.IsMasterClient || CutsceneSyncManager.Instance.IsBusy)
        {
            return;
        }

        CutsceneSyncManager.Instance.PlayForAll(
            clipName: GetEndingClipName(), 
            timeoutSec: 0f, 
            masterOnlyOnAllFinished: () =>
            {
                GameManager.Instance.StageComplete();
            }
        );
    }
    
    [PunRPC]
    private void RPC_StageComplete()
    {
        GameManager.Instance.StageComplete();
    }

    /// <summary>
    /// 현재 맵의 엔딩 영상명 반환
    /// </summary>
    private string GetEndingClipName()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene + "Ending";
    }
}
