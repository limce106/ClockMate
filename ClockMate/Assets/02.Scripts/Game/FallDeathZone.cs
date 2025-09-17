using DefineExtension;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallDeathZone : MonoBehaviour
{
    [SerializeField] ParticleSystem poofEffect;
    private void OnTriggerEnter(Collider other)
    {
        if(other.IsPlayerCollider())
        {
            if (SceneManager.GetActiveScene().name == "ClockTower")
                character.ChangeState<DeadState>(Define.Battle.DeathType.Fall);
            else
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
