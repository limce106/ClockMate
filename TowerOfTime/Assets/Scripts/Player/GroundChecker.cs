using UnityEngine;

public class GroundChecker
{
    private readonly Transform _target;
    private readonly LayerMask _groundLayer;
    private readonly float _radius;
    private readonly float _distance;
    private readonly float _offset;

    public GroundChecker(Transform target, float radius, float distance, float offset, LayerMask layer)
    {
        _target = target;
        _radius = radius;
        _distance = distance;
        _offset = offset;
        _groundLayer = layer;
    }

    public bool IsGrounded(out RaycastHit hit)
    {
        Vector3 start = _target.position + Vector3.up * _offset;
        Vector3 end = _target.position + Vector3.down * _offset;

        return Physics.CapsuleCast(start, end, _radius, Vector3.down, out hit, _distance, _groundLayer);    
    }

    public bool IsGrounded() => IsGrounded(out _);
}