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

    private Rigidbody _rb;
    private RollingStoneSpawner _rollingStoneSpawner;
    private SoundHandle _stoneSfxHandle = default;
    [SerializeField] ParticleSystem _stoneDustEffect;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rollingStoneSpawner = FindObjectOfType<RollingStoneSpawner>();
    }

    public void Initialize(float torque, float returnTime)
    {
        _torqueForce = torque;
        _returnTime = returnTime;

        _stoneDustEffect.Play();
        ResetPhysics();
        StartCoroutine(ReturnAfterDelay());
    }

    private void OnEnable()
    {
        _stoneDustEffect.Play();
        ResetPhysics();
        StartCoroutine(ReturnAfterDelay());
    }

    void FixedUpdate()
    {
        Roll();
    }

    void Roll()
    {
        _rb.AddTorque(transform.right * _torqueForce);
    }

    void ResetPhysics()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(_returnTime);
        ReturnRollingStone();
    }

    private void ReturnRollingStone()
    {
        _stoneDustEffect.Stop();
        _rollingStoneSpawner.StartCoroutine(_rollingStoneSpawner.PlayDestroyStoneEffect(transform.position, transform.rotation));

        SoundManager.Instance.PlaySfx(key: "rock_break", pos: transform.position, volume: 0.4f);

        _rollingStoneSpawner.rollingStonePool.Return(this);

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
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !_stoneSfxHandle.IsValid)
        {
            _stoneSfxHandle = SoundManager.Instance.PlaySfx(
                key: "rock_fall",
                loop: true,
                pos: transform.position,
                sync: false,
                volume: 0.6f);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SoundManager.Instance.Stop(_stoneSfxHandle);
            _stoneSfxHandle = default;
        }
    }
}
