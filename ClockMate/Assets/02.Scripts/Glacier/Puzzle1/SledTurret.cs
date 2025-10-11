using UnityEngine;
using static Define.Character;

public class SledTurret : MonoBehaviour
{
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private string fireSfxKey;

    private void Update()
    {
        if (GameManager.Instance.SelectedCharacter != CharacterName.Milli) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
            SoundManager.Instance.PlaySfx(key: fireSfxKey, pos: transform.position, volume: 1f);
        }
    }

    private void Fire()
    {
        ITurretTarget target = targetDetector.CurrentTarget;
        if (target is null) return;
        
        target.OnHit();
        targetDetector.RemoveTarget(target);
    }
}
