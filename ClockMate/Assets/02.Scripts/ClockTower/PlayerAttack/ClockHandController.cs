using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ClockHandController : MonoBehaviourPun
{
    private Rigidbody _rb;
    private IAClockHand iAClockHand;

    private const float ControllerOffset = 1.2f;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _rb.isKinematic = true;

        iAClockHand = GetComponentInChildren<IAClockHand>();
    }

    [PunRPC]
    public void RPC_Rotate(float rotationAmount)
    {
        transform.root.Rotate(0f, rotationAmount, 0f);
    }

    [PunRPC]
    public void RPC_DetachController(int controllerViewID)
    {
        PhotonView controllerView = PhotonView.Find(controllerViewID);
        if (controllerView == null) return;

        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] controllerColliders = controllerView.GetComponents<Collider>();

        foreach (var controllerCol in controllerColliders)
        {
            foreach (var handCol in handColliders)
            {
                Physics.IgnoreCollision(controllerCol, handCol, false);
            }
        }

        foreach (var handCol in handColliders)
        {
            handCol.enabled = true;
        }

        controllerView.GetComponent<Rigidbody>().isKinematic = false;
        _rb.isKinematic = true;

        controllerView.transform.SetParent(null);
        controllerView.GetComponent<PhotonTransformView>().enabled = true;
    }

    [PunRPC]
    public void RPC_AttachController(int controllerViewID)
    {
        PhotonView controllerView = PhotonView.Find(controllerViewID);
        if (controllerView == null) return;

        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] controllerColliders = controllerView.GetComponents<Collider>();

        foreach (var controllerCol in controllerColliders)
        {
            foreach (var handCol in handColliders)
            {
                // 시계 바늘과 플레이어의 충돌 무시
                Physics.IgnoreCollision(controllerCol, handCol, true);
            }
        }

        foreach (var handCol in handColliders)
        {
            handCol.enabled = false;
        }

        controllerView.GetComponent<Rigidbody>().isKinematic = true;

        PhotonTransformView photonTransformView = controllerView.GetComponent<PhotonTransformView>();
        photonTransformView.enabled = false;

        _rb.isKinematic = false;

        controllerView.transform.SetParent(iAClockHand.meshRenderer.transform);

        float originControllerY = controllerView.transform.position.y;
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 toPlayer = controllerView.transform.position - iAClockHand.meshRenderer.transform.position;
        float side = Vector3.Dot(toPlayer, right);
        Vector3 attachDir = side >= 0 ? right : -right;

        Vector3 targetPos = iAClockHand.transform.position + attachDir * ControllerOffset;
        targetPos.y = originControllerY;

        controllerView.transform.position = targetPos;
        controllerView.transform.rotation = Quaternion.LookRotation(-attachDir);
    }
}
