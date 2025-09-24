using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefineExtension;

[RequireComponent(typeof(Rigidbody))]
public class RollingStone : MonoBehaviourPun
{
    public float _torqueForce;
    private float _returnHeight;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(float torque, float returnHeight)
    {
        _torqueForce = torque;
        _returnHeight = returnHeight;

        ResetPhysics();
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
        rb.AddTorque(transform.right * _torqueForce);
    }

    void ResetPhysics()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void CheckReturn()
    {
        if(transform.position.y <= _returnHeight)
        {
            ReturnRollingStone();
        }
    }

    private void ReturnRollingStone()
    {
        RollingStoneSpawner rollingStoneSpawner = FindObjectOfType<RollingStoneSpawner>();
        rollingStoneSpawner.rollingStonePool.Return(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.IsPlayerCollider())
        {
            CharacterBase character = collision.gameObject.GetComponent<CharacterBase>();

            if (!character.IsDizzy)
            {
                character.StartCoroutine(character.ApplyDizzy(3f));
            }
        }
    }
}
