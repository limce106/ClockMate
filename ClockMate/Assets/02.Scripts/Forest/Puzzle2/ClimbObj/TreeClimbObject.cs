using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeClimbObject : ClimbObjectBase
{
    private const float surfaceOffset = 0.1f;   // ǥ�鿡�� ������ �Ÿ�
    private const float rayOriginBackOffset = 0.1f;   // �ʹ� ����� ��ġ���� Raycast�� ��� ���� �浹���� �ʰų� ���ο��� ���۵ǹǷ� ��¦ �ڷ� �������� ����

    public override void AttachTo(CharacterBase character)
    {
        // ������Ʈ���� ���� ����� ������ ���̱�
        float halfHeight = character.transform.localScale.y * 0.5f;
        Vector3 origin = character.transform.position - character.transform.forward * rayOriginBackOffset + Vector3.up * halfHeight;
        Vector3 direction = character.transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f, LayerMask.GetMask("Climbable")))
        {
            Vector3 surfacePoint = hit.point + hit.normal * surfaceOffset;
            character.transform.position = surfacePoint;

            // ������Ʈ ������ �ٶ󺸰�
            Vector3 forwardDir = -hit.normal;
            forwardDir.y = 0;
            character.transform.forward = forwardDir.normalized;
        }
        else
        {
            // Raycast ���� �� ClosestPoint�� ���� ��ġ ���ϱ�
            Collider col = GetComponent<Collider>();
            Vector3 climbPoint = col.ClosestPoint(character.transform.position) + attachOffset;
            character.transform.position = climbPoint;

            Vector3 dirToCenter = (transform.position - character.transform.position).normalized;
            dirToCenter.y = 0;
            character.transform.forward = dirToCenter;
        }
    }
}
