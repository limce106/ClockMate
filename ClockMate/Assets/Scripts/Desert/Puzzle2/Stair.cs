using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class Stair : MonoBehaviourPun
{
    private float moveDistance;
    public float moveSpeed = 2f;

    // 위치 비교 허용 오차
    private const float PositionThreshold = 0.01f;

    private Vector3 targetPos;
    private bool shouldMove = false;

    [SerializeField]
    private PressurePlate linkedPlate;

    private void Awake()
    {
        moveDistance = CalculateTotalChildWidth();
        targetPos = transform.position + Vector3.right * moveDistance;
    }

    void Update()
    {
        if(linkedPlate.IsFullyPressed && !shouldMove)
        {
            gameObject.SetActive(true);
            linkedPlate.LockState();

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsInRoomAndReady())
            {
                photonView.RPC("RPC_Move", RpcTarget.All);
            }
            else
            {
                Move();
            }
        }

        if (shouldMove)
        {
            MoveTowardsTarget();
        }
    }

    public void Move()
    {
        shouldMove = true;
    }

    [PunRPC]
    public void RPC_Move()
    {
        shouldMove = true;
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, targetPos) < PositionThreshold)
        {
            transform.position = targetPos;
            shouldMove = false;
        }
    }

    private float CalculateTotalChildWidth()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 0f;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach(Renderer renderer in renderers)
        {
            float childMinX = renderer.bounds.min.x;
            float childMaxX = renderer.bounds.max.x;

            minX = Mathf.Min(minX, childMinX);
            maxX = Mathf.Max(maxX, childMaxX);
        }

        return maxX - minX;
    }
}
