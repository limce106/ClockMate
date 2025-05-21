using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondFloorButton : MonoBehaviour
{
    private bool isPlayerColliding = false;
    public Stair stair;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isPlayerColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerColliding = false;
        }
    }

    void Update()
    {
        if(isPlayerColliding && Input.GetKeyDown(KeyCode.E))
        {
            stair.gameObject.SetActive(true);
            stair.Move();
        }
    }
}
