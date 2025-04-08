using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    public GameObject HourPrefab;
    public GameObject MilliPrefab;


    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // 서로 다른 캐릭터로 플레이
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(HourPrefab.name, spawnPoint1.position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(MilliPrefab.name, spawnPoint2.position, Quaternion.identity);
        }
    }
}
