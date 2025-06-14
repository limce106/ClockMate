using System.Collections;
using Define;
using DefineExtension;
using UnityEngine;
using Photon.Pun;

public class MovingBlock : ResettableBase
{
    private PhotonView photonView;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private Coroutine _moveCoroutine;
    
    [Header("Moving Block Properties")]
    [SerializeField] private Block.MovingDirection moveDirection = Block.MovingDirection.Right;
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool isLoop = true;
    [SerializeField] private float waitTimeAtEdge = 0.5f;
    [SerializeField] private bool startAutomatically = true;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private bool _movingForward = true;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();

        if (startAutomatically)
        {
            if (NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
                photonView.RPC("RPC_StartMoving", RpcTarget.AllBuffered);
            else
                StartMoving();
        }
    }

    protected override void Init()
    {
        _startPoint = transform.position;
        _endPoint = _startPoint + moveDirection.GetMovingDirectionVector() * moveDistance;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.CompareTag("Player")) return;
        other.transform.SetParent(transform); // 캐릭터도 블럭 따라 움직이도록 설정
        if (!startAutomatically) // 캐릭터가 밟으면 움직임 시작하도록
        {
            if (NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
            {
                photonView.RPC("RPC_StartMoving", RpcTarget.AllBuffered);

            }
            else
            {
                StartMoving();
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!other.collider.CompareTag("Player")) return;
        other.transform.SetParent(null); // 부모 해제
    }

    private void StartMoving()
    {
        if (_moveCoroutine != null) return;
        
        _moveCoroutine = StartCoroutine(MoveRoutine());
    }

    [PunRPC]
    private void RPC_StartMoving()
    {
        StartMoving();
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTimeAtEdge);
            
            Vector3 target = _movingForward ? _endPoint : _startPoint;

            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;

            if (!isLoop)
            {
                yield break;
            }

            _movingForward = !_movingForward;
        }
    }

    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }


    [PunRPC]
    public override void ResetObject()
    {
        if (this == null) return;

        gameObject.SetActive(true);

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        if (startAutomatically)
        {
            StartMoving();
        }
    }
}
