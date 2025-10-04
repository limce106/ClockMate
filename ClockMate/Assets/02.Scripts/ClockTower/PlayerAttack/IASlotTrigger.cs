using Photon.Pun;
using UnityEngine;

public class IASlotTrigger : MonoBehaviour, IInteractable
{
    private Cog _cog;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CharacterBase character = GameManager.Instance.Characters[GameManager.Instance.SelectedCharacter];
            character.InteractionDetector.TryInteract();
        }
    }

    public bool CanInteract(CharacterBase character)
    {
        return true;
    }

    public void OnInteractAvailable() { }
    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        _cog.photonView.RPC(nameof(_cog.RPC_ReportFitCog), RpcTarget.MasterClient, character.photonView.ViewID);
        gameObject.SetActive(false);
        return true;
    }

    public void SetCog(Cog cog)
    {
        _cog = cog;
    }
}
