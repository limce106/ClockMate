using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefineExtension;

[RequireComponent(typeof(Rigidbody))]
public class RollingStone : MonoBehaviourPun
{
    public float _torqueForce;
    private float _returnTime;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(float torque, float returnTime)
    {
        _torqueForce = torque;
        _returnTime = returnTime;

        ResetPhysics();
        StartCoroutine(ReturnAfterDelay());
    }

    private void OnEnable()
    {
        ResetPhysics();
        StartCoroutine(ReturnAfterDelay());
    }

    void FixedUpdate()
    {
        Roll();
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

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(_returnTime);
        ReturnRollingStone();
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
                SoundManager.Instance.PlaySfx(key: "hit", pos: transform.position, volume: 0.7f);
            }
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SoundManager.Instance.PlaySfx(key: "rock_fall", pos: transform.position, volume: 0.7f);
        }
    }
}
