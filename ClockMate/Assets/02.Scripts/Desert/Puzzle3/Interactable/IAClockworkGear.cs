using System;
using Photon.Pun;

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
            clipName: "DesertEnding", 
            timeoutSec: 0f, 
            masterOnlyOnAllFinished: () =>
            {
                // TODO: 다음 씬으로 넘어가는 로직
                // 임시 엔딩
                photonView.RPC(nameof(RPC_ActivateEndingUI), RpcTarget.All);
            }
        );
    }
    
    [PunRPC]
    private void RPC_ActivateEndingUI()
    {
        UIManager.Instance.Show<UIEnding>("UIEnding");
    }
}
