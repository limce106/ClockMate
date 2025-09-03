using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineFreeLook freeLookCamera;

    public void SetTarget()
    {
        freeLookCamera = GetComponent<CinemachineFreeLook>();

        string characterName = GameManager.Instance?.SelectedCharacter.ToString();
        GameObject player = GameObject.FindWithTag(characterName);

        freeLookCamera.Follow = player.transform;
        freeLookCamera.LookAt = player.transform;
    }
}
