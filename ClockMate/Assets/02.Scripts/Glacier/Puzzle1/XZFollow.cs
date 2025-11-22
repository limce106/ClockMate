using UnityEngine;

public class XZFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private bool followY;

    private void Update()
    {
        // XZ만 따라가기
        float y = transform.position.y;
        if (followY) y = followTarget.position.y;
        transform.position = new Vector3(
            followTarget.position.x,
            y,
            followTarget.position.z
        );

        // LookAt 방향(수평)
        Vector3 dir = lookTarget.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            // LookRotation으로 목표 회전 계산
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // 현재 rotation의 X, Z는 유지
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Vector3 targetEuler = targetRot.eulerAngles;

            // Y만 변경하고 X, Z는 유지
            transform.rotation = Quaternion.Euler(
                currentEuler.x,
                targetEuler.y, 
                currentEuler.z 
            );
        }
    }
}
