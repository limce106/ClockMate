using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TreeClimbObject : ClimbObjectBase
{
    private const float surfaceOffset = 0.1f;   // 표면에서 떨어진 거리
    private const float rayOriginBackOffset = 0.1f;   // 너무 가까운 위치에서 Raycast를 쏘면 벽에 충돌하지 않거나 내부에서 시작되므로 살짝 뒤로 물러나기 위함

    public float attachOffset = 0.5f;

    public override void AttachTo(CharacterBase character)
    {
        Vector3 attachPoint;

        if (character.transform.position.y < transform.position.y)
        {
            // 오브젝트와의 가장 가까운 지점에 붙이기
            float halfHeight = character.transform.localScale.y * 0.5f;
            Vector3 origin = character.transform.position - character.transform.forward * rayOriginBackOffset + Vector3.up * halfHeight;
            Vector3 direction = character.transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 2f, LayerMask.GetMask("Climbable")))
            {
                Vector3 forwardDir = -hit.normal;
                forwardDir.y = 0;
                forwardDir.Normalize();

                Vector3 offset = -forwardDir * attachOffset;
                attachPoint = hit.point + hit.normal * surfaceOffset + offset;
                attachPoint.y = bottomY;

                character.transform.position = attachPoint;
                // 오브젝트 방향을 바라보게
                character.transform.forward = forwardDir;
            }
            else
            {
                // Raycast 실패 시 ClosestPoint로 부착 위치 구하기
                Collider col = GetComponent<Collider>();
                attachPoint = col.ClosestPoint(character.transform.position);

                Vector3 dirToCenter = transform.position - character.transform.position;
                dirToCenter.y = 0;
                dirToCenter.Normalize();

                Vector3 offset = -dirToCenter * attachOffset;
                attachPoint += offset;
                attachPoint.y = bottomY;

                character.transform.position = attachPoint;
                character.transform.forward = dirToCenter;
            }
        }
        else
        {
            Vector3 forwardXZ = character.transform.forward;
            forwardXZ.y = 0;
            forwardXZ.Normalize();

            float radius = GetComponent<Collider>().bounds.extents.x;
            Vector3 offsetDir = -forwardXZ * attachOffset;

            Vector3 attachXZ = transform.position + forwardXZ * radius - offsetDir;
            attachPoint = new Vector3(attachXZ.x, topY, attachXZ.z);

            character.transform.position = attachPoint;
            character.transform.forward = -forwardXZ;
        }
    }
}
