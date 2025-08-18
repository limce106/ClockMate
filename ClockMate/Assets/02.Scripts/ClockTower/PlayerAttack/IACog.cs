using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class IACog : MonoBehaviourPun, IInteractable
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public bool CanInteract(CharacterBase character)
    {
        return true;
    }
    
    public void OnInteractAvailable() { }
    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        return true;
    }
}
