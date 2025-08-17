using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GiantFlower : ResettableBase, IPunObservable
{
    [Header("����")]
    public float sensitivity = 1.0f;    // ���߿� ���� �ΰ���
    public float maxAngle = 20f;        // �ִ� ���� ����
    private float _rotationSpeed = 30.0f; // �������� ����

    [Header("�÷��̾ ���� ����ġ")]
    public float hourWeight = 1.5f;
    public float milliWeight = 1.0f;

    [Header("�ٱ�")]
    public Animator steamAnimator;

    private Rigidbody _rb;
    private bool _isLocked = false;
    private bool _hasTilted = false;
    private Vector3 _initialPosition;

    private const float LevelTolerance = 2f;    // ���� ��� ����

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
        ApplyRotation(totalTorque);

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

            Vector3 localPos = transform.InverseTransformPoint(player.position);
            Vector2 torque = new Vector2(localPos.x, localPos.z) * weight;

            totalTorque += torque;
        }

        return totalTorque;
    }

    void ApplyRotation(Vector2 totalTorque)
    {
        //float angleX = Mathf.Clamp(totalTorque.y * sensitivity, -maxAngle, maxAngle);
        //float angleZ = Mathf.Clamp(totalTorque.x * sensitivity, -maxAngle, maxAngle);
        
        //Quaternion targetRotation = Quaternion.Euler(angleX, 0f, -angleZ);
        //transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);

        _rb.AddTorque(new Vector3(totalTorque.y, 0f, -totalTorque.x) * sensitivity, ForceMode.Force);

        if (_rb.angularVelocity.magnitude > maxAngle)
        {
            _rb.angularVelocity = _rb.angularVelocity.normalized * maxAngle;
        }
    }

    /// <summary>
    /// �� �÷��̾� ��� �ö�� ���¿��� �������� Ȯ��
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
