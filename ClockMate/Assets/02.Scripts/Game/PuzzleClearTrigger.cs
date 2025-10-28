using DefineExtension;
using UnityEngine;

public class PuzzleClearTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.IsPlayerCollider())
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (!character.photonView.IsMine) return;

            GameManager.Instance.StageComplete();
        }
    }
}
