using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DesertPuzzle3Manager: MonoBehaviour
{
    [SerializeField] private MonsterController[] monsters;
    [SerializeField] private IABattery[] batteries;
    [SerializeField] private float batteryRegenTime;
    
    private int _monsterCount;

    public void Awake()
    {
        Init();
    }

    private void Init()
    {
        foreach (MonsterController monster in monsters)
        {
            monster.OnMonsterDied += HandleMonsterDeath;
        }

        foreach (IABattery battery in batteries)
        {
            battery.OnUse += StartRegenCoroutine;
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
            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 spawnPos = monster.transform.position;       
                PhotonNetwork.Instantiate("Items/Key", spawnPos + Vector3.up, Quaternion.identity);
            }
            foreach (IABattery battery in batteries)
            {
                battery.gameObject.SetActive(false);
            }
            StopAllCoroutines();
        }
    }

    private void StartRegenCoroutine(IABattery battery)
    {
        StartCoroutine(WaitAndActivate(battery.gameObject));
    }
    
    private IEnumerator WaitAndActivate(GameObject batteryGO)
    {
        yield return new WaitForSeconds(batteryRegenTime);
        
        Debug.Log(batteryGO.name + " 리젠");
        batteryGO.SetActive(true);
    }
}
