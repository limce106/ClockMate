using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollingStone : MonoBehaviourPun
{
    public float torqueForce = 10f;
    private Rigidbody rb;

    private const float ReturnHeight = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ResetPhysics();
    }

    void FixedUpdate()
    {
        Roll();
        CheckReturn();
    }

    void Roll()
    {
        rb.AddTorque(transform.forward * torqueForce);
    }

    void ResetPhysics()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void CheckReturn()
    {
        if(transform.position.y <= ReturnHeight)
        {
            RollingStoneSpawner rollingStoneSpawner = FindObjectOfType<RollingStoneSpawner>();
            rollingStoneSpawner.rollingStonePool.Return(this);
        }
    }
}
