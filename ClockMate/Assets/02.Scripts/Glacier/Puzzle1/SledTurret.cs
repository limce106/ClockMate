using UnityEngine;

public class SledTurret : MonoBehaviour
{
    [SerializeField] private TargetDetector targetDetector;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
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
