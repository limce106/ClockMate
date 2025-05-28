using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class FallingBlock : ResettableBase, IPunObservable
{
    // 서버 관련 필드
    private PhotonView photonView;
    private bool isFalling; // 떨어짐 상태 동기화용 변수

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Color _initialColor;

    private MeshRenderer _meshRenderer;
    private Material _materialInstance;

    private Vector3 _destroyPoint;

    private Coroutine _fallCoroutine;

    [Header("Falling Block Properties")]
    [SerializeField] private float fallDelay = 1.5f;
    [SerializeField] private float fallSpeed = 7f;
    [SerializeField] private float destroyYThreshold = 10f;
    [SerializeField] private Color delayColor = Color.red;
    [SerializeField] private Color fallingColor = Color.black;

    protected override void Init()
    {
        photonView = GetComponent<PhotonView>();

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _materialInstance = _meshRenderer.material;
        }

        _destroyPoint = new Vector3(transform.position.x, transform.position.y - destroyYThreshold, transform.position.z);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_fallCoroutine != null || !other.collider.CompareTag("Player")) return;
        // 밟은 순간의 애니메이션 재생 코드 추가 (필요하다면!)
        Debug.Log("블럭 밟음");

        if(PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            if (photonView.IsMine)
            {
                photonView.RPC("RPC_StartFalling", RpcTarget.All);
            }
        }
        else
        {
            StartFallingLocally();
        }
    }

    private void StartFallingLocally()
    {
        _materialInstance.color = delayColor;
        _fallCoroutine = StartCoroutine(FallRoutine());
    }

    [PunRPC]
    public void RPC_StartFalling()
    {
        StartFallingLocally();
    }

    private IEnumerator FallRoutine()
    {
        yield return new WaitForSeconds(fallDelay);

        Debug.Log("블럭 추락 시작");
        isFalling = true;
        _materialInstance.color = fallingColor;
        // 떨어지는 순간의 애니메이션 재생 (이것도 필요시)

        while (true)
        {
            while (Vector3.Distance(transform.position, _destroyPoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _destroyPoint, fallSpeed * Time.deltaTime);
                yield return null;
            }
            Debug.Log("블럭 비활성화");
            gameObject.SetActive(false); // 비활성화
            isFalling = false;
            yield break;
        }
    }

    // 초기 상태 저장
    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialColor = _materialInstance.color;
    }

    // 초기화 로직
    public override void ResetObject()
    {
        if (this == null) return;
        gameObject.SetActive(true);

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;

        if (_materialInstance != null)
            _materialInstance.color = _initialColor;

        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        isFalling = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_materialInstance.color);
            stream.SendNext(isFalling);
            stream.SendNext(gameObject.activeSelf);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            _materialInstance.color = (Color)stream.ReceiveNext();
            isFalling = (bool)stream.ReceiveNext();
            bool isActive = (bool)stream.ReceiveNext();

            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
            }
        }
    }
}