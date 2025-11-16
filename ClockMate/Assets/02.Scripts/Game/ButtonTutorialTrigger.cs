using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTutorialTrigger : MonoBehaviour
{
    bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.IsPlayerCollider())
        {
            CharacterBase character = other.GetComponentInParent<CharacterBase>();
            if (!character.photonView.IsMine) return;

            UIManager.Instance.Show<UITutorial>("UITutorial", false);
            isTriggered = true;
        }
    }
}
