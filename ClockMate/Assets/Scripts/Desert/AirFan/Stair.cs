using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stair : MonoBehaviour
{
    private float moveDistance;
    public float moveSpeed = 2f;

    // 위치 비교 허용 오차
    private const float PositionThreshold = 0.01f;

    private Vector3 targetPos;
    private bool shouldMove = false;

    private void Awake()
    {
        moveDistance = gameObject.transform.localScale.x;
        targetPos = transform.position + Vector3.right * moveDistance;
    }

    void Update()
    {
        if(shouldMove)
        {
            MoveTowardsTarget();
        }
    }

    public void Move()
    {
        shouldMove = true;
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, targetPos) < PositionThreshold)
        {
            transform.position = targetPos;
            shouldMove = false;
        }
    }
}
