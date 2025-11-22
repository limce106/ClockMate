using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Define.Character;

public class SledController : MonoBehaviourPunCallbacks, IPunObservable
{
    [field: SerializeField] public SledTurret Turret { get; private set; }
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform[] pathPoints;   // 길 웨이포인트들 (순서대로)
    [SerializeField] private float maxPathYawAngle = 45f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityMultiplier;
    [field: SerializeField] public bool IsMoving {get; private set;}
 
    [Header("효과음 & 이펙트")] 
    [SerializeField] private string movingSfxKey = "sledding_on_snow";
    [SerializeField] private string jumpSfxKey= "sled_jump";
    [SerializeField] private string landSfxKey = "sled_ground_hit";
    [SerializeField] private string splashSfxKey = "water_splash";
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private ParticleSystem landVfx;
    [SerializeField] private ParticleSystem splashVfx;
    
    private Rigidbody _rb;
    private int _currentPathIndex;
    private bool _isGrounded;
    private bool _jumpPressed;

    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            if (value)
            {
                landVfx.gameObject.SetActive(true);
                SoundManager.Instance.PlaySfx(key: landSfxKey, pos: transform.position, volume: sfxVolume, sync: true);
            }
            _isGrounded = value;
        }
    }
    private Vector3 _initPos;
    private Quaternion _initRot;
    
    public SledHp Hp {get; private set;}
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // 썰매 소유권은 아워가 가지도록 설정
        int houActorNr = GameManager.Instance.Characters[CharacterName.Hour].photonView.OwnerActorNr;
        if (photonView.OwnerActorNr != houActorNr)
        {
            photonView.TransferOwnership(houActorNr);
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        //_currentYaw = 0f;
        Hp = GetComponent<SledHp>();
        _isGrounded = false;
        _initPos = transform.position;
        _initRot = transform.rotation;
    }

    private void Update()
    {
        if (!IsMoving || !photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpPressed = true;
        }
    }
    
    private void FixedUpdate()
    {
        if (!IsMoving || !photonView.IsMine) return;

        MoveForward(); 
        HandleTurn();

        if (_jumpPressed && IsGrounded)
        {
            Jump();
        }
        _jumpPressed = false;
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        if (IsGrounded) return;
        
        Vector3 extraGravity = Physics.gravity * (gravityMultiplier - 1f);
        _rb.AddForce(extraGravity, ForceMode.Acceleration);
    }

    /// <summary>
    /// 썰매가 바라보는 정면 방향으로 이동
    /// </summary>
    private void MoveForward()
    {
        _rb.MovePosition(_rb.position + transform.forward * (moveSpeed * Time.fixedDeltaTime));
    }
    
    /// <summary>
    /// 좌/우 회전 처리
    /// </summary>
    private void HandleTurn()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.A)) turn = -1f; // 왼쪽
        if (Input.GetKey(KeyCode.D)) turn = 1f; // 오른쪽
        if (turn == 0f) return;

        // 길 방향
        Vector3 pathDir = GetPathDirection();
        Vector3 sledDir = transform.forward;
        pathDir.y = 0;
        sledDir.y = 0;

        if (pathDir.sqrMagnitude < 0.001f) return;

        
        float currentAngle = Vector3.SignedAngle(pathDir, sledDir, Vector3.up); // 길 기준 현재 각도
        float deltaYaw = turn * rotationSpeed * Time.fixedDeltaTime; // 입력에 따른 회전량
        float newAngle = Mathf.Clamp(currentAngle + deltaYaw, -maxPathYawAngle, maxPathYawAngle); // 최종 각도 클램프

        // 실제로 적용할 회전량
        float appliedDelta = newAngle - currentAngle;
        if (Mathf.Abs(appliedDelta) < 0.0001f)
            return;

        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, appliedDelta, 0f));
    }
    
    private Vector3 GetPathDirection()
    {
        if (pathPoints == null || pathPoints.Length < 2)
            return transform.forward; // 안전장치

        // 현재 인덱스 보정
        _currentPathIndex = Mathf.Clamp(_currentPathIndex, 0, pathPoints.Length - 2);

        // 다음 포인트까지의 방향
        Vector3 from = pathPoints[_currentPathIndex].position;
        Vector3 to   = pathPoints[_currentPathIndex + 1].position;

        // 썰매가 현재 포인트보다 많이 지나갔으면 다음 세그먼트로
        Vector3 toSled = transform.position - from;
        Vector3 seg    = to - from;
        if (Vector3.Dot(toSled, seg) > seg.sqrMagnitude)
        {
            // 다음 세그먼트로 진행
            if (_currentPathIndex < pathPoints.Length - 2)
                _currentPathIndex++;
        }

        Vector3 dir = (pathPoints[_currentPathIndex + 1].position - pathPoints[_currentPathIndex].position);
        dir.y = 0;
        return dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;
    }
    
    private void Jump()
    {
//        Debug.Log("Jump pressed");
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        if (NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_Jump), RpcTarget.Others);
        }
    }
    
    [PunRPC]
    private void RPC_Jump()
    {
        Jump();
    }
    
    public void SetSledMoveState(bool value)
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetSledMoveState), RpcTarget.All, value);
        }
        else if (!NetworkManager.Instance.IsInRoomAndReady())
        {
            RPC_SetSledMoveState(value);
        }
    }
    
    [PunRPC]
    private void RPC_SetSledMoveState(bool value)
    {
        IsMoving = value;
        SoundManager.Instance.StopByKey(movingSfxKey);
        if (IsMoving)
        {
            SoundManager.Instance.PlaySfx(
                key: movingSfxKey, volume: 0.5f, loop: true);
        }
    }

    public void ResetTransform()
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && !photonView.IsMine) return;
        transform.position = _initPos;
        transform.rotation = _initRot;
        SoundManager.Instance.PlaySfx(
            key: movingSfxKey, volume: 0.5f, loop: true);
    }
    
    public void Drown()
    {
        var splashPos = new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z);
        Instantiate(splashVfx, splashPos, Quaternion.identity);
        SoundManager.Instance.PlaySfx(key: splashSfxKey, volume: 0.4f, sync: false);
        SoundManager.Instance.StopByKey(movingSfxKey);
        Hp.TakeDamage(100);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
