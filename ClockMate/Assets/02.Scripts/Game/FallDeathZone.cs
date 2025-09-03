using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.IsPlayerCollider())
        {
            CharacterBase character = other.gameObject.GetComponentInParent<CharacterBase>();
            character.ChangeState<DeadState>();
        }
    }
}
