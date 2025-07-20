using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TreeClimbObject : ClimbObjectBase
{
    private const float surfaceOffset = 0.1f;   // ǥ�鿡�� ������ �Ÿ�
    private const float rayOriginBackOffset = 0.1f;   // �ʹ� ����� ��ġ���� Raycast�� ��� ���� �浹���� �ʰų� ���ο��� ���۵ǹǷ� ��¦ �ڷ� �������� ����

    public float attachOffsetX = -0.5f;

    public override void AttachTo(CharacterBase character)
    {
        Vector3 attachPoint;

        if (character.transform.position.y < gameObject.transform.position.y)
        {
            // ������Ʈ���� ���� ����� ������ ���̱�
            float halfHeight = character.transform.localScale.y * 0.5f;
            Vector3 origin = character.transform.position - character.transform.forward * rayOriginBackOffset + Vector3.up * halfHeight;
            Vector3 direction = character.transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f, LayerMask.GetMask("Climbable")))
            {
                attachPoint = hit.point + hit.normal * surfaceOffset;
                attachPoint.x += attachOffsetX;
                attachPoint.y = bottomY;
                character.transform.position = attachPoint;

                // ������Ʈ ������ �ٶ󺸰�
                Vector3 forwardDir = -hit.normal;
                forwardDir.y = 0;
                character.transform.forward = forwardDir.normalized;
            }
            else
            {
                // Raycast ���� �� ClosestPoint�� ���� ��ġ ���ϱ�
                Collider col = GetComponent<Collider>();
                attachPoint = col.ClosestPoint(character.transform.position);
                attachPoint.x += attachOffsetX;
                attachPoint.y = bottomY;
                character.transform.position = attachPoint;

                Vector3 dirToCenter = (transform.position - character.transform.position).normalized;
                dirToCenter.y = 0;
                character.transform.forward = dirToCenter;
            }
        }
        else
        {
            Vector3 forwardXZ = character.transform.forward;
            forwardXZ.y = 0;
            forwardXZ.Normalize();

            float radius = GetComponent<Collider>().bounds.extents.x;
            Vector3 attachXZ = transform.position + forwardXZ * (radius + -attachOffsetX);
            attachPoint = new Vector3(attachXZ.x, topY, attachXZ.z);

            character.transform.position = attachPoint;
            character.transform.forward = -forwardXZ;
        }
    }
}
