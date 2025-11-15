using System.Collections;
using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class IAIceBallTrigger : MonoBehaviourPunCallbacks, IInteractable    
{
    [SerializeField] private IceBall iceBall;

    private void Start()
    {
        StartCoroutine(DisableIfMilliRoutine());
    }

    
    private IEnumerator DisableIfMilliRoutine()
    {
        // 네트워크 준비 대기
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom);
        if (GameManager.Instance.SelectedCharacter == CharacterName.Hour) yield break;

        if (TryGetComponent(out Collider triggerCol))
        {
            triggerCol.enabled = false;
        }
    }
    
    public bool CanInteract(CharacterBase character)
    {
        return character is Hour && !iceBall.IsControlled;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        if (character is not Hour hour) return false;

        // 트리거와 물리 충돌 비활성화
        //IgnoreCollider(true, character);
        TryGetComponent(out Collider triggerCol);
        triggerCol.enabled = false;
        
        iceBall.StartControl(hour);
        iceBall.OnControlEnd -= EnableCollider;
        iceBall.OnControlEnd += EnableCollider;

        return true;
    }

    private void IgnoreCollider(bool ignore, CharacterBase character)
    {
        // if (iceBall.TryGetComponent(out Collider col))
        // {
        //     foreach (var chCol in character.GetComponentsInChildren<Collider>())
        //     {
        //         Physics.IgnoreCollision(col, chCol, ignore);
        //     }
        // }

        if (TryGetComponent(out Collider triggerCol))
        {
            triggerCol.enabled = !ignore;
        }
    }

    private void EnableCollider(bool enable)
    {
        if (TryGetComponent(out Collider triggerCol))
        {
            triggerCol.enabled = enable;
        }
    }
}
