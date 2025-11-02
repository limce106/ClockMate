using UnityEngine;
using Cinemachine;
using Photon.Pun;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ThirdPersonFollowPOVCam : MonoBehaviour
{
    [Header("Follow Settings")]
    public float cameraDistance = 3f;

    [Tooltip("카메라 추적 감속")]
    public Vector3 followDamping = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("카메라가 바라볼 오프셋")]
    public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

    private CinemachineVirtualCamera vcam;
    private Cinemachine3rdPersonFollow followComponent;

    public void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        followComponent = vcam.AddCinemachineComponent<Cinemachine3rdPersonFollow>();

        // 위치 제어
        followComponent.CameraDistance = cameraDistance;
        followComponent.Damping = followDamping;
        followComponent.ShoulderOffset = lookAtOffset;

        Transform camRoot = transform.parent;
        vcam.Follow = camRoot;
        vcam.LookAt = camRoot;
    }
}