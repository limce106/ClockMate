using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PolarBearController : MonoBehaviourPun
{
    [SerializeField] private Transform sled;
    [SerializeField] private float followDistance;
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform front;
    
    private Rigidbody _rb;
    private bool _isJumping;
    private LayerMask _layerMask;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        _layerMask = LayerMask.GetMask("Default");
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        // 썰매를 따라서 이동
        Vector3 direction = (sled.position - transform.position);
        direction.y = 0f;

        if (direction.magnitude > followDistance) return;
        
        if (NeedToJump())
        {
            Jump();
        }
        else 
        {
            Move(direction.normalized);
        }
    }

    private void Move(Vector3 direction)
    {
        _rb.MovePosition(_rb.position + direction * (Time.fixedDeltaTime * moveSpeed));
        transform.forward = direction;
    }

    private bool NeedToJump()
    {
        Ray ray = new Ray(front.position, Vector3.down);
        return !_isJumping && Physics.Raycast(ray, 5.5f, _layerMask) == false;
    }

    private void Jump()
    {
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _isJumping = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            _isJumping = false;
        }
    }
}
