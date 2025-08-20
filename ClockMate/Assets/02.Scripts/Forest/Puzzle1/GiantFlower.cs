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
    private float _rotationSpeed = 20.0f; // 기울어지는 각도

    [Header("플레이어별 하중 가중치")]
    public float hourWeight = 1.5f;
    public float milliWeight = 1.0f;

    [Header("줄기")]
    public Animator steamAnimator;

    private Rigidbody _rb;
    private bool _isLocked = false;
    private bool _hasTilted = false;
    private Vector3 _initialPosition;

    private const float LevelTolerance = 2f;    // 수평 허용 오차
    
    private Vector2 _smoothedTorque;
    public float smoothingFactor = 0.5f;
    private Dictionary<Transform, Vector3> _smoothedPlayerPositions = new Dictionary<Transform, Vector3>();

    private List<Transform> _playersOnFlower = new List<Transform>();

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(_isLocked) 
            return;

        Vector2 totalTorque = CalculateTotalTorque();
        _smoothedTorque = Vector2.Lerp(_smoothedTorque, totalTorque, smoothingFactor);
        ApplyRotation(_smoothedTorque);

        if (!_hasTilted)
        {
            float tiltAmount = Quaternion.Angle(transform.rotation, Quaternion.identity);
            if (tiltAmount > LevelTolerance)
                _hasTilted = true;
        }
    }

    Vector2 CalculateTotalTorque()
    {
        Vector2 totalTorque = Vector2.zero;

        foreach (Transform player in _playersOnFlower)
        {
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

    void ApplyRotation(Vector2 totalTorque)
    {
        if (totalTorque.magnitude < 0.05f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.fixedDeltaTime * _rotationSpeed);
            return;
        }
        
        float angleX = Mathf.Clamp(totalTorque.y * sensitivity, -maxAngle, maxAngle);
        float angleZ = Mathf.Clamp(totalTorque.x * sensitivity, -maxAngle, maxAngle);
        
        Quaternion targetRotation = Quaternion.Euler(angleX, 0f, -angleZ);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time. fixedDeltaTime * _rotationSpeed);
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

        float angleDiff = Quaternion.Angle(transform.localRotation, Quaternion.identity);

        return angleDiff < LevelTolerance;
    }

    public void Lock()
    {
        _isLocked = true;
        transform.localEulerAngles = Vector3.zero;
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
            }
        }
    }

    protected override void SaveInitialState()
    {
        _initialPosition = transform.position;
    }

    public override void ResetObject()
    {
        transform.position = _initialPosition;
        _isLocked = false;
        _hasTilted = false;
        _playersOnFlower.Clear();
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
