using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SnowballShooter : MonoBehaviourPun
{
    [SerializeField] private Transform sled;
    [SerializeField] private SnowballPoolModule pool;
    [SerializeField] private Transform[] snowballGenPositions;
    [SerializeField] private TargetDetector targetDetector;
    
    [SerializeField] private float fireInterval;
    [SerializeField] private float snowballSpeed;
    
    private float _fireTimer;
    private bool[,] _attackPatterns;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _attackPatterns = new bool[,]
        {
            {false, false, false, true, true, true, false, false, false},
            {false, true, false, false, true, false, false, true, false},
            {true, false, false, false, true, false, false, false, true},
            {false, false, true, false, true, false, true, false, false},
            {true, false, true, false, true, false, true, false, true},
            {true, false, true, false, false, false, true, false, true},
        };
    }

    private void Update()
    {
        //if (!PhotonNetwork.IsMasterClient) return;

        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireInterval)
        {
            _fireTimer -= fireInterval;
            FireSnowball();
        }
    }

    private void FireSnowball()
    {
        int pattern = Random.Range(0, 6);

        for (int i = 0; i < 9; i++)
        {
            if (!_attackPatterns[pattern, i]) continue;
            Snowball snowball = pool.Get();
            snowball.Launch(snowballGenPositions[i], sled, snowballSpeed);
            targetDetector.AddTarget(snowball);
        }
    }
}

