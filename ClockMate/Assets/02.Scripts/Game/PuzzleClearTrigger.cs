using DefineExtension;
using UnityEngine;

public class PuzzleClearTrigger : MonoBehaviour
{
    bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.IsPlayerCollider())
        {
            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            if (!character.photonView.IsMine) return;

            GameManager.Instance.StageComplete();

            isTriggered = true;
        }
    }
}
