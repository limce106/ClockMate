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
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(isPlayerColliding)
            {
                stair.gameObject.SetActive(true);
                stair.Move();
            }
            
        }
    }
}
