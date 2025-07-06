using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GiantFlower : ResettableBase
{
    [Header("����")]
    public float sensitivity = 1.0f;    // ���߿� ���� �ΰ���
    public float maxAngle = 20f;        // �ִ� ���� ����
    private float _rotationSpeed = 30.0f; // �������� ����

    [Header("�÷��̾ ���� ����ġ")]
    public float hourWeight = 1.5f;
    public float milliWeight = 1.0f;

    private bool _isLocked = false;
    private Vector3 _initialPosition;

    private const float LevelTolerance = 2f;    // ���� ��� ����

    private List<Transform> _playersOnFlower = new List<Transform>();

    void Update()
    {
        if(_isLocked) 
            return;

        Vector2 totalTorque = CalculateTotalTorque();
        ApplyRotation(totalTorque);
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
        float angleX = Mathf.Clamp(totalTorque.y * sensitivity, -maxAngle, maxAngle);
        float angleZ = Mathf.Clamp(totalTorque.x * sensitivity, -maxAngle, maxAngle);
        
        Quaternion targetRotation = Quaternion.Euler(angleX, 0f, -angleZ);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    /// <summary>
    /// �� �÷��̾� ��� �ö�� ���¿��� �������� Ȯ��
    /// </summary>
    public bool IsLevel()
    {
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
        if(collision.gameObject.CompareTag("Hour") || collision.gameObject.CompareTag("Milli"))
        {
            if(!_playersOnFlower.Contains(collision.transform))
            {
                _playersOnFlower.Add(collision.transform);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hour") || collision.gameObject.CompareTag("Milli"))
        {
            if (_playersOnFlower.Contains(collision.transform))
            {
                _playersOnFlower.Remove(collision.transform);
            }
        }
    }

    protected override void SaveInitialState()
    {
        GiantFlowerManager giantFlowerManager = GameObject.Find("GiantFlowerManager").GetComponent<GiantFlowerManager>();
        if (giantFlowerManager.giantFlowers[0] == this)
        {
            _initialPosition = transform.position;
        }
        else
        {
            _initialPosition = transform.position + Vector3.up * GiantFlowerManager.dropOffsetY;
        }
        _isLocked = false;
        _playersOnFlower.Clear();
    }

    public override void ResetObject()
    {
        transform.position = _initialPosition;
        _isLocked = false;
        _playersOnFlower.Clear();
    }
}
