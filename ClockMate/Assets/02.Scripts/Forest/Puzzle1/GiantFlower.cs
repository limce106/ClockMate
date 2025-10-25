using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GiantFlower : ResettableBase, IPunObservable
{
    [Header("설정")]
    public float sensitivity = 1.0f;    // 하중에 대한 민감도
    public float maxAngle = 20f;        // 최대 기울기 각도
    private float _rotationSpeed = 1.2f; // 기울어지는 각도

    [Header("플레이어별 하중 가중치")]
    public float hourWeight = 20f;
    public float milliWeight = 15f;

    [Header("줄기")]
    public GameObject sideSteam;
    public Animator steamAnimator;

    public Rigidbody _rb;
    private bool _isLocked = false;
    private bool _hasTilted = false;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private const float LevelTolerance = 5f;    // 수평 허용 오차
    
    private SoundHandle _rotationSfxHandle = default;

    private Dictionary<Transform, Vector3> _smoothedPlayerPositions = new Dictionary<Transform, Vector3>();
    private List<Transform> _playersOnFlower = new List<Transform>();

    private Quaternion _networkRotation;

    private void Start()
    {
        _initialRotation = transform.localRotation;
        _networkRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        if(_isLocked) 
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터에서 회전값 갱신
            Vector2 totalTorque = CalculateTotalTorque();
            ApplyTorqueAndLimitRotation(totalTorque);
            _networkRotation = _rb.rotation;
        }
        else
        {
            // 다른 클라이언트는 마스터 회전값만 적용
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, _networkRotation, Time.fixedDeltaTime * 10f));
        }

        UpdateTiltStateAndSound();
    }

    /// <summary>
    /// 현재 꽃 위에 있는 플레이어들의 하중과 위치를 기반으로 토크 계산
    /// </summary>
    Vector2 CalculateTotalTorque()
    {
        Vector2 totalTorque = Vector2.zero;

        foreach (Transform player in _playersOnFlower)
        {
            // 플레이어 위치 초기화
            if (!_smoothedPlayerPositions.ContainsKey(player))
            {
                _smoothedPlayerPositions[player] = player.position;
            }
            
            _smoothedPlayerPositions[player] = Vector3.Lerp(_smoothedPlayerPositions[player], player.position, 0.5f);
            
            float weight = 0f;

            if (player.CompareTag("Hour"))
            {
                weight = hourWeight;
            }
            else if (player.CompareTag("Milli"))
            {
                weight = milliWeight;
            }
            else
            {
                continue;
            }

            Vector3 localPos = transform.InverseTransformPoint(_smoothedPlayerPositions[player]);
            Vector2 torque = new Vector2(localPos.x, localPos.z) * weight;

            totalTorque += torque;
        }

        return totalTorque;
    }

    /// <summary>
    /// 토크 기반으로 목표 회전 계산 및 물리 회전 적용
    /// </summary>
    void ApplyTorqueAndLimitRotation(Vector2 totalTorque)
    {
        Vector3 currentEuler = transform.localEulerAngles;
        currentEuler.x = currentEuler.x > 180 ? currentEuler.x - 360 : currentEuler.x;
        currentEuler.z = currentEuler.z > 180 ? currentEuler.z - 360 : currentEuler.z;

        float targetX = Mathf.Clamp(totalTorque.y * sensitivity, -maxAngle, maxAngle); // 전방-후방
        float targetZ = Mathf.Clamp(-totalTorque.x * sensitivity, -maxAngle, maxAngle); // 좌-우

        // 작은 변화 무시
        const float deadZone = 0.3f;
        if (Mathf.Abs(targetX - currentEuler.x) < deadZone) targetX = currentEuler.x;
        if (Mathf.Abs(targetZ - currentEuler.z) < deadZone) targetZ = currentEuler.z;

        float newX = Mathf.Lerp(currentEuler.x, targetX, Time.fixedDeltaTime * _rotationSpeed);
        float newZ = Mathf.Lerp(currentEuler.z, targetZ, Time.fixedDeltaTime * _rotationSpeed);

        _rb.MoveRotation(Quaternion.Euler(newX, 0f, newZ));
    }

    /// <summary>
    /// 현재 기울기 상태 확인
    /// </summary>
    void UpdateTiltStateAndSound()
    {
        // 현재 각도
        float currentAngle = Quaternion.Angle(transform.localRotation, _initialRotation);

        // 한 번이라도 기울어졌는지 확인
        if (!_hasTilted && currentAngle > LevelTolerance)
        {
            _hasTilted = true;
        }

        float angularSpeed = _rb.angularVelocity.magnitude;

        // 일정 각도 이상 움직이는 경우에만 재생
        if (currentAngle > LevelTolerance && angularSpeed > 0.1f)
        {
            if (!_rotationSfxHandle.IsValid)
            {
                _rotationSfxHandle = SoundManager.Instance.PlaySfx(
                    key: "giantflower_rotate",
                    loop: true,
                    pos: transform.position,
                    sync: false,
                    volume: 0.8f);
            }
        }
        else
        {
            if (_rotationSfxHandle.IsValid)
            {
                SoundManager.Instance.Stop(_rotationSfxHandle);
                _rotationSfxHandle = default;
            }
        }
    }

    /// <summary>
    /// 두 플레이어 모두 올라온 상태에서 수평인지 확인
    /// </summary>
    public bool IsLevel()
    {
        if (!_hasTilted)
            return false;

        bool isHourOn = false;
        bool isMilliOn = false;

        foreach (Transform player in _playersOnFlower)
        {
            if (player.CompareTag("Hour"))
                isHourOn = true;
            if (player.CompareTag("Milli"))
                isMilliOn = true;
        }

        if (!isHourOn || !isMilliOn)
            return false;

        float angleDiff = Quaternion.Angle(transform.localRotation, _initialRotation);

        // 각속도도 매우 낮아야 완전히 멈춘 것으로 간주
        float angularSpeed = _rb.angularVelocity.magnitude;

        // 수평 허용 오차 내에 있고, 움직임이 거의 없을 때 true 반환
        return angleDiff < LevelTolerance && angularSpeed < 0.1f;
    }

    /// <summary>
    /// 꽃 수평 고정
    /// </summary>
    public void Lock()
    {
        _isLocked = true;

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        transform.localRotation = Quaternion.identity;

        if (_rotationSfxHandle.IsValid)
        {
            SoundManager.Instance.Stop(_rotationSfxHandle);
            _rotationSfxHandle = default;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.IsPlayerCollider())
        {
            if(!_playersOnFlower.Contains(collision.transform))
            {
                _playersOnFlower.Add(collision.transform);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.IsPlayerCollider())
        {
            if (_playersOnFlower.Contains(collision.transform))
            {
                _playersOnFlower.Remove(collision.transform);

                if (_smoothedPlayerPositions.ContainsKey(collision.transform))
                {
                    _smoothedPlayerPositions.Remove(collision.transform);
                }
            }
        }
    }

    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public override void ResetObject()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;

        _isLocked = false;
        _hasTilted = false;
        _playersOnFlower.Clear();
        _smoothedPlayerPositions.Clear();

        if (_rotationSfxHandle.IsValid)
        {
            SoundManager.Instance.Stop(_rotationSfxHandle);
            _rotationSfxHandle = default;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (_rb == null) return;

        if (stream.IsWriting)
        {
            stream.SendNext(_networkRotation);
        }
        else
        {
            _networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
