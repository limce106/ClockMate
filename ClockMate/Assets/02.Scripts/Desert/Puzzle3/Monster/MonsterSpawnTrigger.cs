using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [FormerlySerializedAs("desertPuzzel3Manager")] [FormerlySerializedAs("monsterManager")] [SerializeField] private DesertPuzzle3Manager desertPuzzle3Manager;
    private void OnTriggerEnter(Collider other)
    {
        desertPuzzle3Manager.SpawnMonsters();
        this.gameObject.SetActive(false);
    }
}
