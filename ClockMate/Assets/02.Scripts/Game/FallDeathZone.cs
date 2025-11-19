using DefineExtension;
using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    [SerializeField] ParticleSystem poofEffect;
    
    [SerializeField] bool isWater;
    [SerializeField] ParticleSystem splashVfx;
    [SerializeField] private string splashSfxKey = "water_splash";
    [SerializeField] private float splashVolume = 1.0f;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.IsPlayerCollider())
        {
            if (isWater)
            {
                var splashPos = new Vector3(other.transform.position.x, other.transform.position.y - 0.3f, other.transform.position.z);
                Instantiate(splashVfx, splashPos, Quaternion.identity);
                SoundManager.Instance.PlaySfx(key: splashSfxKey, pos: transform.position, volume: splashVolume, sync: false);
            }
            CharacterBase character = other.gameObject.GetComponentInParent<CharacterBase>();

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
