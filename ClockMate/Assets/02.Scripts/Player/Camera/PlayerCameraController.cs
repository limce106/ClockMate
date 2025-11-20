using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float sensitivityX = 300f;
    public float sensitivityY = 250f;

    [Tooltip("카메라 상하 회전 최소/최대 각도")]
    public float minPitch = -45f;
    public float maxPitch = 80f;

    [Tooltip("카메라 회전이 따라오는 속도")]
    public float rotationSmoothTime = 0.05f;

    public bool canRotate = true;

    private float yaw;
    private float pitch;
    private float smoothYaw;
    private float smoothPitch;
    private float yawVelocity;
    private float pitchVelocity;

    void Start()
    {
        // 자기 캐릭터가 아니라면 끄기
        PhotonView photonView = GetComponentInParent<PhotonView>();
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void LateUpdate()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if(!canRotate)
        {
            smoothYaw = yaw;
            smoothPitch = pitch;
            yawVelocity = 0f;
            pitchVelocity = 0f;

            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * sensitivityX * Time.deltaTime;
        pitch -= mouseY * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawVelocity, rotationSmoothTime);
        smoothPitch = Mathf.SmoothDamp(smoothPitch, pitch, ref pitchVelocity, rotationSmoothTime);

        // 회전 적용
        transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
    }
}

