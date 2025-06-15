using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [SerializeField] private MonsterManager monsterManager;
    private void OnTriggerEnter(Collider other)
    {
        monsterManager.SpawnMonsters();
        this.gameObject.SetActive(false);
    }
}
