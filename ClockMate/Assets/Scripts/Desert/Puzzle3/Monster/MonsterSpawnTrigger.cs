using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [FormerlySerializedAs("monsterManager")] [SerializeField] private DesertPuzzel3Manager desertPuzzel3Manager;
    private void OnTriggerEnter(Collider other)
    {
        desertPuzzel3Manager.SpawnMonsters();
        this.gameObject.SetActive(false);
    }
}
