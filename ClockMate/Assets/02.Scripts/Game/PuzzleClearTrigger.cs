using DefineExtension;
using UnityEngine;

public class PuzzleClearTrigger : MonoBehaviour
{
    bool isTriggered = false;
    [Header("클리어 처리해야하는 스테이지 ID")]
    [SerializeField] private int clearStageId;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || GameManager.Instance.CurrentStage.ID != clearStageId) return;

        if (other.IsPlayerCollider())
        {
            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            if (!character.photonView.IsMine) return;

            GameManager.Instance.StageComplete();

            isTriggered = true;
        }
    }
}
