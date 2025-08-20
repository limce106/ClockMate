using System;
using UnityEngine;

public class BreakPoint : MonoBehaviour, ITurretTarget
{ 
    private ParticleSystem _destroyEffect;
    private DestroyableIce _ice;
    private TargetDetector _targetDetector;
    
    public void Init(DestroyableIce ice, TargetDetector targetDetector)
    {
        _ice = ice;
        _ice.UiIceBreakPoint.SetImage(this);
        _targetDetector = targetDetector;
        _targetDetector.AddTarget(this);
        _destroyEffect = Resources.Load<ParticleSystem>("Effects/PointDestroy");
    }
    
    public void OnHit()
    {
        Instantiate(_destroyEffect, transform.position, Quaternion.identity);
        _ice.ReduceBreakPoint();
        _ice.UiIceBreakPoint.HideImage(this);
        gameObject.SetActive(false);
        _targetDetector.RemoveTarget(this);
    }
}
