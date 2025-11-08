using DefineExtension;
using UnityEngine;

public class PuzzleClearTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.IsPlayerCollider())
        {
            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            if (!character.photonView.IsMine) return;

            GameManager.Instance.StageComplete();
            

        }
    }

    private void ShowQuest()
    {
        PuzzleHUD puzzleHUD = GameObject.FindAnyObjectByType<PuzzleHUD>();
        puzzleHUD.SetQuestActive(true);
    }
}
