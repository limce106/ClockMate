using DefineExtension;
using UnityEngine;

public class PuzzleClearTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.IsPlayerCollider())
        {
            GameManager.Instance.StageComplete();
        }
    }
}
