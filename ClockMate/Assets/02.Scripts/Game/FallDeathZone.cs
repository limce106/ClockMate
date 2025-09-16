using DefineExtension;
using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    [SerializeField] ParticleSystem poofEffect;
    private void OnTriggerEnter(Collider other)
    {
        if(other.IsPlayerCollider())
        {
            CharacterBase character = other.gameObject.GetComponentInParent<CharacterBase>();
            //character.ChangeState<DeadState>();
            Vector3 loadPosition = GameManager.Instance.CurrentStage.LoadPositions[character.Name];
            character.transform.position = loadPosition;
            if (poofEffect != null)
            {
                Instantiate(poofEffect, loadPosition, Quaternion.identity);
            }
            character.ChangeState<IdleState>();
        }
    }
}
