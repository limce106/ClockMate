using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondFloorButton : MonoBehaviour
{
    private bool isPlayerColliding = false;
    public Stair stair;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerColliding = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerColliding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerColliding = false;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isPlayerColliding)
        {
            stair.gameObject.SetActive(true);

            if (NetworkManager.Instance.IsInRoomAndReady())
            {
                stair.photonView.RPC("RPC_Move", RpcTarget.All);
            }
            else
            {
                stair.Move();
            }
        }
    }
}
