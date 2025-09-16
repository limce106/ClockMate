using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FallDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.IsPlayerCollider())
        {
            CharacterBase character = other.gameObject.GetComponentInParent<CharacterBase>();

            if (SceneManager.GetActiveScene().name == "ClockTower")
                character.ChangeState<DeadState>(Define.Battle.DeathType.Fall);
            else
                character.ChangeState<DeadState>();
        }
    }
}
