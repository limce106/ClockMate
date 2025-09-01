using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using DefineExtension;

public class AirFanDoorController : MonoBehaviourPun
{
    [SerializeField] private float _moveDistance = 4f;
    [SerializeField] private float _moveSpeed = 2f;

    // 위치 비교 허용 오차
    private const float PositionThreshold = 0.01f;

    [SerializeField] private PressurePlate _linkedPlate;
    [SerializeField] private GameObject _firstFloorDoor;
    [SerializeField] private GameObject _secondFloorDoor;

    private Vector3 _firstDoorTargetPos;
    private Vector3 _secondDoorTargetPos;

    private bool _isMoving = false;

    private void Awake()
    {
        _firstDoorTargetPos = _firstFloorDoor.transform.position + Vector3.up * _moveDistance;
        _secondDoorTargetPos = _secondFloorDoor.transform.position + Vector3.up * _moveDistance;
    }

    void Update()
    {
        if(_linkedPlate.IsFullyPressed && !_isMoving)
        {
            _linkedPlate.SetLockState(true);

            NetworkExtension.RunNetworkOrLocal(
            MoveDoors,
            () => photonView.RPC(nameof(RPC_MoveDoors), RpcTarget.All)
            );
        }
    }

    public void MoveDoors()
    {
        _isMoving = true;
        StartCoroutine(MoveDoorsCoroutine());
    }

    [PunRPC]
    public void RPC_MoveDoors()
    {
        MoveDoors();
    }

    private IEnumerator MoveDoorsCoroutine()
    {
        while(true)
        {
            bool isFirstDoorAtTarget = Vector3.Distance(_firstFloorDoor.transform.position, _firstDoorTargetPos) < PositionThreshold;
            bool isSecondDoorAtTarget = Vector3.Distance(_secondFloorDoor.transform.position, _secondDoorTargetPos) < PositionThreshold;

            if(isFirstDoorAtTarget && isSecondDoorAtTarget)
            {
                _isMoving = false;
                yield break;
            }

            _firstFloorDoor.transform.position = Vector3.MoveTowards(_firstFloorDoor.transform.position, _firstDoorTargetPos, _moveSpeed * Time.deltaTime);
            _secondFloorDoor.transform.position = Vector3.MoveTowards(_secondFloorDoor.transform.position, _secondDoorTargetPos, _moveSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
