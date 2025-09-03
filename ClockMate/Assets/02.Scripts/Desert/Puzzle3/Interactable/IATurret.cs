using Cinemachine;
using DefineExtension;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 터렛 상호작용.
/// Milli만 조작 가능하며 조작 중에는 캐릭터 조작이 비활성화된다.Q
/// </summary>
public class IATurret : MonoBehaviourPun, IInteractable
{
    [SerializeField] private GameObject turretHead;
    [SerializeField] private Transform attackStartPos;
    [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private int maxChargeLevel;
    [field: SerializeField] public int ChargeLevel { get; private set; }
    
    [SerializeField] private float turretRotateSpeed = 8f;
    [SerializeField] private float yawMin = -60f;
    [SerializeField] private float yawMax = 60f;
    [SerializeField] private float pitchMin = -20f;
    [SerializeField] private float pitchMax = 50f;

    [SerializeField] private LineRenderer attackLineRenderer;
    [SerializeField] private float attackRange = 100f;
    [SerializeField] private LayerMask hitMask; // 대상 레이어
    
    [SerializeField] private string chargeSfxKey;
    [SerializeField] private float chargeSfxVolume;
    [SerializeField] private string fireSfxKey;
    [SerializeField] private float fireSfxVolume;
    
    private GameObject _indicator; // 타겟 표시
    
    private bool _isOccupied; // 터렛 조작 중 여부
    private CharacterBase _character; // 조작 중인 캐릭터
    private MonsterController _currentTarget;
    private UINotice _uiNotice;
    private UITurretAcive _uiTurretActive;
    
    private Sprite _dropSprite;
    private string _dropString;

    private void Awake()
    { 
        Init();   
    }

    private void Init()
    {
        _isOccupied = false;
        _character = null;
        _dropSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _dropString = "나가기";
        if (attackLineRenderer != null)
        {
            attackLineRenderer.positionCount = 2;
            attackLineRenderer.enabled = false;
        }
        
        _indicator = Instantiate(Resources.Load<GameObject>("UI/Indicator"), this.transform);
        _indicator.transform.localPosition = new Vector3(0, 0.2f, 0);
        _indicator.SetActive(false);
    }

    private void Update()
    {
        if (!_isOccupied) return;

        HandleTurretRotation();
        UpdateAttackTarget();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryFireWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitTurret();
        }

    }

    public bool CanInteract(CharacterBase character)
    {
        return character is Milli && !_isOccupied;
    }

    public void OnInteractAvailable()
    {
        
    }

    public void OnInteractUnavailable()
    {
        
    }

    public bool Interact(CharacterBase character)
    {
        _isOccupied = true;
        character.InputHandler.enabled = false; // 캐릭터 조작 모두 비활성화
        _character = character;
        
        // 터렛 조작 그만두기 UI 표시
        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_dropSprite);
        _uiNotice.SetText(_dropString);
        
        // 상호작용 탐지되지 않도록 collider 비활성화
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }
        
        camera.Priority = 100;
        attackLineRenderer.enabled = true;

        _uiTurretActive = UIManager.Instance.Show<UITurretAcive>("UITurretActive");
        _uiTurretActive?.UpdateChargeImg(ChargeLevel);

        return true;
    }

    /// <summary>
    /// 터렛 조작 해제 처리.
    /// </summary>
    private void ExitTurret()
    {
        _isOccupied = false;
        _character.InputHandler.enabled = true;
        _character = null;
        camera.Priority = 0;
        attackLineRenderer.enabled = false;

        // 터렛 UI 닫기
        
        _uiTurretActive.Reset();
        UIManager.Instance.Close(_uiTurretActive);
        UIManager.Instance.Close(_uiNotice);
        _uiTurretActive = null;
        _uiNotice = null;
        
        // collider 다시 활성화
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }
    }

    /// <summary>
    /// 터렛 head 회전 처리 (키보드 입력)
    /// - 좌우(Yaw): Left/Right 혹은 A/D 키로 조작 (각도 제한 포함)
    /// - 상하(Pitch): Up/Down 혹은 W/S 키로 조작 (각도 제한 포함)
    /// </summary>
    private void HandleTurretRotation()
    {
        float yawInput = 0f;
        float pitchInput = 0f;

        // Yaw 입력 처리
        if (Input.GetKey(KeyCode.A))
            yawInput = -1f;
        else if (Input.GetKey(KeyCode.D))
            yawInput = 1f;

        // Pitch 입력 처리
        if (Input.GetKey(KeyCode.W))
            pitchInput = 1f;
        else if (Input.GetKey(KeyCode.S))
            pitchInput = -1f;

        // 회전량 계산
        float yawAmount = yawInput * turretRotateSpeed * Time.deltaTime;
        float pitchAmount = pitchInput * turretRotateSpeed * Time.deltaTime;

        // yaw 회전 (Y축)
        Vector3 currentRotation = turretHead.transform.localEulerAngles;
        currentRotation.y += yawAmount;
        currentRotation.y = ClampAngle(currentRotation.y, yawMin, yawMax); // 좌우 회전 제한

        // pitch 회전 (X축)
        currentRotation.x -= pitchAmount;
        currentRotation.x = ClampAngle(currentRotation.x, pitchMin, pitchMax); // 상하 회전 제한
        
        // 터렛 head에 회전 적용
        Vector3 eulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);
        NetworkExtension.RunNetworkOrLocal(
            () => LocalRotate(eulerAngles),
            () => photonView.RPC(nameof(RPC_RotateTurret), RpcTarget.All, eulerAngles)
        );
    }
    
    private void LocalRotate(Vector3 eulerAngles)
    {
        turretHead.transform.localEulerAngles = eulerAngles;
    }
    
    [PunRPC]
    public void RPC_RotateTurret(Vector3 eulerAngles)
    {
        LocalRotate(eulerAngles);
    }

    /// <summary>
    /// Euler 각도를 -180~180으로 정규화 후 min/max로 제한.
    /// </summary>
    private float ClampAngle(float angle, float min, float max)
    {
        angle = angle > 180f ? angle - 360f : angle;
        return Mathf.Clamp(angle, min, max);
    }

    private void UpdateAttackTarget()
    {
        Vector3 startPoint = attackStartPos.position;
        Vector3 direction = attackStartPos.forward;

        Ray ray = new Ray(startPoint, direction);
        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(ray, out hit, attackRange, hitMask))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Monster"))
            {
                hit.collider.TryGetComponent(out _currentTarget);
                if (NetworkManager.Instance.IsInRoomAndReady())
                {
                    photonView.RPC(nameof(RPC_SetTurretTarget), RpcTarget.Others, _currentTarget.photonView.ViewID);
                }
                Debug.Log("조준 상대" + _currentTarget.name);
                _indicator.transform.SetParent(_currentTarget.transform, false);
                _indicator.SetActive(true);
            }
            else
            {
                if (_currentTarget is not null)
                {
                    _currentTarget = null;
                    _indicator.SetActive(false);
                    _indicator.transform.SetParent(transform, false);
                }
            }
        }
        else
        {
            endPoint = startPoint + direction * attackRange;
        }

        attackLineRenderer.SetPosition(0, startPoint);
        attackLineRenderer.SetPosition(1, endPoint);
    }
    
    [PunRPC]
    private void RPC_SetTurretTarget(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;

        MonsterController monster = view.GetComponent<MonsterController>();
        if (monster == null) return;
        
        _currentTarget = monster;
    }
    
    /// <summary>
    /// 터렛 공격 시도
    /// </summary>
    private void TryFireWeapon()
    {
        if (ChargeLevel <= 0)
        {
            Debug.Log("Charge가 부족하여 발사할 수 없습니다.");
            // 필요시 UI 피드백 처리
            return;
        }

        NetworkExtension.RunNetworkOrLocal(
            LocalFire,
            () => photonView.RPC(nameof(RPC_FireTurret), RpcTarget.All)
        );
    }

    /// <summary>
    /// 실제 터렛 공격 처리
    /// </summary>
    private void LocalFire()
    {
        ChargeLevel--; // ChargeLevel 1 감소
        _uiTurretActive?.UpdateChargeImg(ChargeLevel);
        SoundManager.Instance.PlaySfx(key: fireSfxKey, pos: transform.position, volume: fireSfxVolume);

        if (_currentTarget is not null)
        {
            _currentTarget.ChangeStateTo<MStateDead>();
        }
        // TODO: 발사체 구현
    }
    
    [PunRPC]
    public void RPC_FireTurret()
    {
        LocalFire();
    }

    public void Charge()
    {
        NetworkExtension.RunNetworkOrLocal(
            LocalCharge,
            () => photonView.RPC(nameof(RPC_ChargeTurret), RpcTarget.All)
        );
    }
    
    private void LocalCharge()
    {
        if (ChargeLevel >= maxChargeLevel) return;
        SoundManager.Instance.PlaySfx(key: chargeSfxKey, volume: chargeSfxVolume);
        _uiTurretActive?.UpdateChargeImg(ChargeLevel);
    }
    
    [PunRPC]
    public void RPC_ChargeTurret()
    {
        LocalCharge();
    }
}
