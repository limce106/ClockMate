using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirFanGateDoorController : MonoBehaviourPun
{
    [SerializeField] private float _moveDistance = 4f;
    [SerializeField] private float _moveSpeed = 2f;

    // 위치 비교 허용 오차
    private const float PositionThreshold = 0.01f;

    [SerializeField] private PressurePlate _linkedPlate;
    [SerializeField] private GameObject _gate;

    private Vector3 _gateTargetPos;

    private bool _isMoving = false;

    private void Awake()
    {
        _gateTargetPos = _gate.transform.position + Vector3.up * _moveDistance;
    }

    void Update()
    {
        if (_linkedPlate != null && _linkedPlate.IsFullyPressed && !_isMoving)
        {
            _linkedPlate.SetLockState(true);

            NetworkExtension.RunNetworkOrLocal(
            MoveGate,
            () => photonView.RPC(nameof(RPC_MoveGate), RpcTarget.All)
            );
        }
    }

    public void MoveGate()
    {
        _isMoving = true;
        StartCoroutine(MoveGateCoroutine());
    }

    [PunRPC]
    public void RPC_MoveGate()
    {
        MoveGate();
    }

    private IEnumerator MoveGateCoroutine()
    {
        while (true)
        {
            bool isGateAtTarget = Vector3.Distance(_gate.transform.position, _gateTargetPos) < PositionThreshold;

            if (isGateAtTarget)
            {
                _isMoving = false;
                yield break;
            }

            _gate.transform.position = Vector3.MoveTowards(_gate.transform.position, _gateTargetPos, _moveSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
