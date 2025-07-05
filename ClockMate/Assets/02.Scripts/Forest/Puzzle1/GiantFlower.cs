using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GiantFlower : MonoBehaviour
{
    [Header("설정")]
    public float sensitivity = 1.0f;    // 하중에 대한 민감도
    public float maxAngle = 20f;        // 최대 기울기 각도
    private float rotationSpeed = 2.0f; // 기울어지는 각도
    private bool isLocked = false;

    [Header("플레이어별 하중 가중치")]
    public float hourWeight = 1.5f;
    public float milliWeight = 1.0f;

    private const float LevelTolerance = 2f;    // 수평 허용 오차

    private List<Transform> playersOnFlower = new List<Transform>();

    void Update()
    {
        if(isLocked) 
            return;

        Vector2 totalTorque = CalculateTotalTorque();
        ApplyRotation(totalTorque);
    }

    Vector2 CalculateTotalTorque()
    {
        Vector2 totalTorque = Vector2.zero;

        foreach (Transform player in playersOnFlower)
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
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// 수평인지 확인
    /// </summary>
    public bool IsLevel()
    {
        Vector3 euler = transform.localEulerAngles;
        euler.x = euler.x > 180f ? euler.x - 360f : euler.x;
        euler.z = euler.z > 180f ? euler.z - 360f : euler.z;

        return Mathf.Abs(euler.x) < LevelTolerance && Mathf.Abs(euler.z) < LevelTolerance;
    }

    public void Lock()
    {
        isLocked = true;
        transform.localEulerAngles = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Hour") || collision.gameObject.CompareTag("Milli"))
        {
            if(!playersOnFlower.Contains(collision.transform))
            {
                playersOnFlower.Add(collision.transform);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hour") || collision.gameObject.CompareTag("Milli"))
        {
            if (playersOnFlower.Contains(collision.transform))
            {
                playersOnFlower.Remove(collision.transform);
            }
        }
    }
}
