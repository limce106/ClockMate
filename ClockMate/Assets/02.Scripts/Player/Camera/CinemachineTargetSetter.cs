using Cinemachine;
using UnityEngine;
using static Define.Character;

public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineFreeLook freeLookCamera;

    public void SetTarget()
    {
        freeLookCamera = GetComponent<CinemachineFreeLook>();

        CharacterName characterName = GameManager.Instance.SelectedCharacter;
        CharacterBase character = GameManager.Instance.Characters[characterName];
        GameObject target = character != null ? character.gameObject : GameObject.FindWithTag(characterName.ToString());
        if (target != null)
        {
            freeLookCamera.Follow = target.transform;
            freeLookCamera.LookAt = target.transform;
            return;
        }
        Debug.LogError($"[CinemachineTargetSetter] No target found for {characterName}");
    }
}
