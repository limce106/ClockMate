using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 두 개의 PressurePlate가 모두 완전히 눌렸을 때 문을 왼족 또는 오른쪽 방향으로 열고,
/// 열리기 시작한 후에는 발판을 고정한다.
/// </summary>
public class PressurePlateGateBlock : ResettableBase
{
    private PhotonView photonView;

    [Header("Door Properties")]
    [SerializeField] private PressurePlate[] linkedPlates;
    [SerializeField] private bool isDoorLeft = true;
    [SerializeField] private float openOffsetX = 5f;
    [SerializeField] private float openSpeed = 2f;
    
    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private Coroutine _openCoroutine;

    private bool _isOpened;
    
    protected override void Init()
    {
        photonView = GetComponent<PhotonView>();
        _isOpened = false;
        _closedPosition = transform.position;
    }

    private void Update()
    {
        if (_isOpened) return;

        if (AllPlatesFullyPressed())
        {
            if(NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
            {
                photonView.RPC("RPC_OpenDoor", RpcTarget.All);
            }
            else
            {
                OpenDoor();
            }
        }
    }

    private bool AllPlatesFullyPressed()
    {
        return linkedPlates.All(plate => plate.IsFullyPressed);
    }
    
    /// <summary>
    /// 문 열기 시작 및 발판 고정
    /// </summary>
    private void OpenDoor()
    {
        if (_openCoroutine != null) return;

        _openCoroutine = StartCoroutine(OpenDoorRoutine());

        foreach (PressurePlate plate in linkedPlates)
        {
            plate.LockState(); // 발판 고정
        }

        _isOpened = true;
    }

    [PunRPC]
    private void RPC_OpenDoor()
    {
        OpenDoor();
    }
    
    /// <summary>
    /// 목표 위치까지 문 이동
    /// </summary>
    private IEnumerator OpenDoorRoutine()
    {
        while (Vector3.Distance(transform.position, _openPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _openPosition, openSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = _openPosition;
    }

    protected override void SaveInitialState()
    {
        Vector3 direction = isDoorLeft ? Vector3.left : Vector3.right;
        _openPosition = _closedPosition + direction * openOffsetX;
    }

    [PunRPC]
    public override void ResetObject()
    {
        if (_openCoroutine != null)
        {
            StopCoroutine(_openCoroutine);
            _openCoroutine = null;
        }

        transform.position = _closedPosition;
        _isOpened = false;
    }
}