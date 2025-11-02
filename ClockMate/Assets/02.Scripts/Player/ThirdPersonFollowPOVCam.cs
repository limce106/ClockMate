using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class ThirdPersonFollowPOVCam : MonoBehaviour
{
    [Header("Follow Settings")]
    public float cameraDistance = 3f;

    public Vector3 followDamping = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

    [Header("POV Settings")]
    public float horizontalSpeed = 400f;
    public float verticalSpeed = 300f;
    public float minPitch = -45f;
    public float maxPitch = 80f;

    private CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;
    public Transform POVTransform => pov != null ? pov.transform : null;

    public void Start()
    {
        // 자기 캐릭터가 아니라면 끄기
        PhotonView photonView = GetComponentInParent<PhotonView>();
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
        }

        vcam = GetComponent<CinemachineVirtualCamera>();

        // 위치 제어
        var transposer = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_XDamping = followDamping.x;
        transposer.m_YDamping = followDamping.y;
        transposer.m_ZDamping = followDamping.z;

        transposer.m_TrackedObjectOffset = lookAtOffset;
        transposer.m_CameraDistance = cameraDistance;

        // 카메라가 대상 위치를 화면 중앙에 유지하도록 설정
        transposer.m_ScreenX = 0.5f;
        transposer.m_ScreenY = 0.5f;

        // 회전 제어
        pov = vcam.AddCinemachineComponent<CinemachinePOV>();
        pov.m_HorizontalAxis.m_MaxSpeed = horizontalSpeed;
        pov.m_VerticalAxis.m_MaxSpeed = verticalSpeed;
        pov.m_VerticalAxis.m_MinValue = minPitch;
        pov.m_VerticalAxis.m_MaxValue = maxPitch;
        pov.m_VerticalAxis.m_Wrap = false;
    }
}