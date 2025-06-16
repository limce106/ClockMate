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

    [SerializeField] private GameObject rootGO;
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

        _destroyPoint = new Vector3(rootGO.transform.position.x, rootGO.transform.position.y - destroyYThreshold, rootGO.transform.position.z);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_fallCoroutine != null || !other.collider.CompareTag("Player")) return;
        // 밟은 순간의 애니메이션 재생 코드 추가 (필요하다면!)
        Debug.Log("블럭 밟음");

        // 서버에 연결됐고 방에 들어왔다면
        if (NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
        {
            photonView.RPC("RPC_StartFalling", RpcTarget.All);
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
            while (Vector3.Distance(rootGO.transform.position, _destroyPoint) > 0.01f)
            {
                rootGO.transform.position = Vector3.MoveTowards(rootGO.transform.position, _destroyPoint, fallSpeed * Time.deltaTime);
                yield return null;
            }
            Debug.Log("블럭 비활성화");
            rootGO.SetActive(false); // 비활성화
            isFalling = false;
            yield break;
        }
    }

    // 초기 상태 저장
    protected override void SaveInitialState()
    {
        _initialPosition = rootGO.transform.position;
        _initialRotation = rootGO.transform.rotation;
        _initialColor = _materialInstance.color;
    }

    // 초기화 로직
    [PunRPC]
    public override void ResetObject()
    {
        if (this == null) return;
        rootGO.SetActive(true);

        rootGO.transform.position = _initialPosition;
        rootGO.transform.rotation = _initialRotation;

        if (_materialInstance != null)
            _materialInstance.color = _initialColor;

        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        isFalling = false;
    }

    /// <summary>
    /// 포톤 네트워크를 통해 블록의 상태(위치, 색상, 활성화 여부, 낙하 상태)를 동기화
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);

            Color color = _materialInstance.color;
            stream.SendNext(color.r);
            stream.SendNext(color.g);
            stream.SendNext(color.b);
            stream.SendNext(color.a);

            stream.SendNext(isFalling);
            stream.SendNext(gameObject.activeSelf);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();

            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext();
            float a = (float)stream.ReceiveNext();
            _materialInstance.color = new Color(r, g, b, a);

            isFalling = (bool)stream.ReceiveNext();
            bool isActive = (bool)stream.ReceiveNext();

            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
            }
        }
    }
}