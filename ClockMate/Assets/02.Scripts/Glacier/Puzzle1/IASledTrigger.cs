using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class IASledTrigger : MonoBehaviourPun, IInteractable
{
    private bool _available = true;
    [SerializeField] private ChaseControlModule chaseControl;
    [SerializeField] private GameObject rootGo;
    
    public bool CanInteract(CharacterBase character)
    {
        return _available;
    }

    public bool Interact(CharacterBase character)
    {
        photonView.RPC(nameof(RPC_OnSledTriggerInteract), RpcTarget.All);
        photonView.RPC(nameof(RPC_DisableTriggerGo), RpcTarget.All);

        return false;
    }

    [PunRPC]
    private void RPC_OnSledTriggerInteract()
    {
        _available = false;
        // 상호작용 감지용 콜라이더 비활성화
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
        SoundManager.Instance.StopAll(SoundType.BGM);
        if (!PhotonNetwork.IsMasterClient) return;
        GameManager.Instance.SetAllCharactersActive(false);
        CutsceneSyncManager.Instance.PlayCinematicForAll(
            "Glacier_Chase_Start",
            0f,
            () =>
            {
                chaseControl.StartChase();
            }
        );
    }
    
    [PunRPC]
    private void RPC_DisableTriggerGo()
    {
        rootGo.SetActive(false);
    }
    
    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }
}
