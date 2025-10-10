using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EffectWrapper : MonoBehaviourPun
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        _particleSystem.Play();
    }

    private void OnEnable()
    {
        _particleSystem.Play();
    }

    public ParticleSystem Particle
    {
        get { return _particleSystem; }
    }
}
