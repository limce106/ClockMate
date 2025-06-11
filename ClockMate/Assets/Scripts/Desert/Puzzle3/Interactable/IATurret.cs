using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 터렛 상호작용.
/// Milli만 조작 가능하며 조작 중에는 캐릭터 조작이 비활성화된다.
/// </summary>
public class IATurret : MonoBehaviour, IInteractable
{
    [SerializeField] private IAChargeStation chargeStation;
    [SerializeField] private GameObject turretHead;
    [SerializeField] private Transform muzzle;
    
    [SerializeField] private float turretRotateSpeed = 8f;
    [SerializeField] private float yawMin = -60f;
    [SerializeField] private float yawMax = 60f;
    [SerializeField] private float pitchMin = -20f;
    [SerializeField] private float pitchMax = 50f;

    [SerializeField] private LineRenderer attackLineRenderer;
    [SerializeField] private float attackRange = 100f;
    [SerializeField] private LayerMask hitMask; // 대상 레이어
    
    private GameObject _indicator; // 타겟 표시
    
    private bool _isOccupied; // 터렛 조작 중 여부
    private CharacterBase _character; // 조작 중인 캐릭터
    private MonsterController _currentTarget;
    private UINotice _uiNotice;
    
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
        _indicator.SetActive(false);
    }

    private void Update()
    {
        if (!_isOccupied) return;

        HandleTurretRotation();
        UpdateAttackTarget();
        // TODO 터렛 발사 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryFireWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitTurret();
            attackLineRenderer.enabled = false;
            // 터렛 조작 나가기 UI 닫기
            UIManager.Instance.Close(_uiNotice);
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
        
        attackLineRenderer.enabled = true;
        
        // TODO 터렛 조작 UI 켜기

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
        // TODO 터렛 조작 UI 닫기
        
        
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
        turretHead.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);
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
        Vector3 startPoint = muzzle.position;
        Vector3 direction = muzzle.forward;

        Ray ray = new Ray(startPoint, direction);
        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(ray, out hit, attackRange, hitMask))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Monster"))
            {
                hit.collider.TryGetComponent(out _currentTarget);
                _indicator.transform.SetParent(hit.collider.transform, false);
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

    /// <summary>
    /// 터렛 공격 시도
    /// </summary>
    private void TryFireWeapon()
    {
        if (chargeStation == null || chargeStation.ChargeLevel <= 0)
        {
            Debug.Log("Charge가 부족하여 발사할 수 없습니다.");
            // 필요시 UI 피드백 처리
            return;
        }

        FireWeapon();
        chargeStation.UseCharged(); // ChargeLevel 1 감소
    }

    /// <summary>
    /// 실제 터렛 공격 처리
    /// </summary>
    private void FireWeapon()
    {
        if (_currentTarget is not null)
        {
            _currentTarget.ChangeStateTo<MStateDead>();
        }

        // TODO: 발사 이펙트, 사운드, UI 피드백 추가
    }
}
