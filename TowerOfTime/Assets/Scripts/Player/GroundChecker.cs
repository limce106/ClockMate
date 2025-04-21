using UnityEngine;

public class GroundChecker
{
    private readonly Collider _collider;
    private readonly LayerMask _groundLayer;
    private readonly float _castDistance;
    private const float Skin = 0.05f; // 오차 보정 값


    public GroundChecker(Collider collider, float castDistance, LayerMask groundLayer)
    {
        _collider = collider;
        _castDistance = castDistance;
        _groundLayer = groundLayer;
    }

    public bool IsGrounded(out RaycastHit hit)
    {
        // 공통적으로 bounds 기준으로 계산
        Vector3 origin = _collider.bounds.center;
        Vector3 extents = _collider.bounds.extents;

        float radius = Mathf.Min(extents.x, extents.z) * 0.95f;
        float height = extents.y;

        // 캐릭터 중앙 기준으로 상하 끝점 계산
        Vector3 point1 = origin + Vector3.up * (height - radius);
        Vector3 point2 = origin - Vector3.up * (height - radius - Skin);

        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, out hit, _castDistance, _groundLayer);
    }

    public bool IsGrounded() => IsGrounded(out _);
}