using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager: MonoBehaviour
{
    // TODO: 몬스터 위치 스폰 위치 & 순찰 지점 확정 후 csv로 관리
    [SerializeField] private MonsterController[] monsters;

    private int _monsterCount;
    private GameObject _keyPrefab;

    public void Awake()
    {
        Init();
    }

    private void Init()
    {
        _keyPrefab = Resources.Load<GameObject>("Items/Key");
        
        foreach (MonsterController monster in monsters)
        {
            monster.OnMonsterDied += HandleMonsterDeath;
        }
    }

    public void SpawnMonsters()
    {
        foreach (MonsterController monster in monsters)
        {
            monster.gameObject.SetActive(true);
        }
        _monsterCount = monsters.Length;
    }

    private void HandleMonsterDeath(MonsterController monster)
    {
        _monsterCount--;
        monster.OnMonsterDied -= HandleMonsterDeath;

        if (_monsterCount <= 0)
        {
            Vector3 spawnPos = monster.transform.position;
            Instantiate(_keyPrefab, spawnPos + Vector3.up, Quaternion.identity);
        }
    }
}
