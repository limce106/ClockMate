using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class SledController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxYawAngle; // 좌우 최대 회전 각도
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
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
    private bool _jumpRequested;
    private bool _wasGrounded;
    private float _currentYaw;
    private bool _hasControl;
    private SledHp _sledHp;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        _jumpRequested = false;
        _currentYaw = 0f;
        _hasControl = true;
        _sledHp = GetComponent<SledHp>();
        _wasGrounded = IsGrounded();
    }

    private void Update()
    {
        if (!IsMoving || !_hasControl) return;
        // 움직이는 중이고 아워라면 
        bool grounded = IsGrounded();
        
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            // 땅에 있을 때 스페이스 눌리면 점프 요청 처리
            _jumpRequested = true;
        }

        if (!_wasGrounded && grounded)
        {
            landVfx.gameObject.SetActive(true);
            SoundManager.Instance.PlaySfx(key: landSfxKey, pos: transform.position, volume: sfxVolume, sync: true);
        }

        _wasGrounded = grounded;
    }

    private void FixedUpdate()
    {
        if (!IsMoving || !_hasControl) return;

        MoveForward(); 
        HandleTurn();

        if (_jumpRequested)
        {
            Jump();
            _jumpRequested = false;
        }
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
        
        float deltaYaw = turn * rotationSpeed * Time.fixedDeltaTime; // 한 프레임마다 회전할 양
        float nextYaw = Mathf.Clamp(_currentYaw + deltaYaw, -maxYawAngle, maxYawAngle); // 회전 누적 제한
        float clampedDelta = nextYaw - _currentYaw; // 제한 반영된 실제 회전량
        _currentYaw = nextYaw; // _currentYaw 갱신

        // 회전 적용
        Quaternion deltaRotation = Quaternion.Euler(Vector3.up * clampedDelta); // y축 기준
        _rb.MoveRotation(_rb.rotation * deltaRotation);
    }

    
    private void Jump()
    {
        Debug.Log("Jump pressed");
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// 썰매가 지면에 닿아있는지 여부를 반환
    /// </summary>
    private bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    public void SetSledMoving(bool value)
    {
        IsMoving = value;
        SoundManager.Instance.StopByKey(movingSfxKey);
        if (IsMoving)
        {
            SoundManager.Instance.PlaySfx(
                key: movingSfxKey, volume: 0.1f, loop: true);
        }
    }

    public void SetControl(bool hasControl)
    {
        _hasControl = hasControl;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        // 물과 충돌하면 바로 추격 재시작
        if(!other.gameObject.CompareTag("Water")) return;
        Instantiate(splashVfx, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySfx(key: splashSfxKey, pos: transform.position, volume: sfxVolume, sync: true);

        _sledHp.TakeDamage(100);
        
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
