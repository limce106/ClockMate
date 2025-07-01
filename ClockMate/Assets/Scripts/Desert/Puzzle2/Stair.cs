using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class Stair : MonoBehaviourPun
{
    private float _moveDistance;
    public float moveSpeed = 2f;

    // 위치 비교 허용 오차
    private const float PositionThreshold = 0.01f;

    private Vector3 _targetPos;
    private bool _shouldMove = false;

    [SerializeField]
    private PressurePlate linkedPlate;

    private void Awake()
    {
        _moveDistance = CalculateTotalChildWidth();
        _targetPos = transform.position + Vector3.right * _moveDistance;
    }

    void Update()
    {
        if(linkedPlate.IsFullyPressed && !_shouldMove)
        {
            gameObject.SetActive(true);
            linkedPlate.SetLockState(true);

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsInRoomAndReady())
            {
                photonView.RPC("RPC_Move", RpcTarget.All);
            }
            else
            {
                Move();
            }
        }

        if (_shouldMove)
        {
            MoveTowardsTarget();
        }
    }

    public void Move()
    {
        _shouldMove = true;
    }

    [PunRPC]
    public void RPC_Move()
    {
        _shouldMove = true;
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, _targetPos) < PositionThreshold)
        {
            transform.position = _targetPos;
            _shouldMove = false;
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
