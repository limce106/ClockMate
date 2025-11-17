using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Define.Character;

public class SledController : MonoBehaviourPunCallbacks, IPunObservable
{
    [field: SerializeField] public SledTurret Turret { get; private set; }
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxYawAngle; // 좌우 최대 회전 각도
    [SerializeField] private float jumpForce;
    [field: SerializeField] public bool IsMoving {get; private set;}
 
    [Header("효과음 & 이펙트")] 
    [SerializeField] private string movingSfxKey = "sledding_on_snow";
    [SerializeField] private string jumpSfxKey= "sled_jump";
    [SerializeField] private string landSfxKey = "sled_ground_hit";
    [SerializeField] private string splashSfxKey = "water_splash";
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private GameObject landVfxGo;
    [SerializeField] private ParticleSystem splashVfx;
    
    private Rigidbody _rb;
//    private float _currentYaw;
    private bool _isGrounded;
    private bool _jumpPressed;

    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            if (value)
            {
                landVfxGo.SetActive(true);
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
        // 회전 입력이 존재할 경우 회전 처리
        
        // float deltaYaw = turn * rotationSpeed * Time.fixedDeltaTime; // 한 프레임마다 회전할 양
        // float nextYaw = Mathf.Clamp(_currentYaw + deltaYaw, -maxYawAngle, maxYawAngle); // 회전 누적 제한
        // float clampedDelta = nextYaw - _currentYaw; // 제한 반영된 실제 회전량
        // _currentYaw = nextYaw; // _currentYaw 갱신
        //
        // // 회전 적용
        // Quaternion deltaRotation = Quaternion.Euler(Vector3.up * clampedDelta); // y축 기준
        
        float deltaYaw = turn * rotationSpeed * Time.fixedDeltaTime;
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, deltaYaw, 0f));
    }

    
    private void Jump()
    {
        Debug.Log("Jump pressed");
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
    }
    
    public void Drown()
    {
        var splashPos = new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z);
        Instantiate(splashVfx, splashPos, Quaternion.identity);
        SoundManager.Instance.PlaySfx(key: splashSfxKey, pos: transform.position, volume: sfxVolume, sync: true);
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
