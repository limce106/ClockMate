using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullAirFanTrigger : MonoBehaviour
{
    private bool isHourInTrigger = false;
    private AirFan airFan;

    void Awake()
    {
        airFan = GetComponentInParent<AirFan>();
    }

    void Update()
    {
        if(isHourInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            airFan.SwitchFan();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null)
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Hour;
        }

        if (characterIdentifier && isTargetCharacter)
        {
            isHourInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null)
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Hour;
        }


        if (characterIdentifier && isTargetCharacter)
        {
            isHourInTrigger = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null)
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Hour;
        }

        if (characterIdentifier && isTargetCharacter)
        {
            isHourInTrigger = true;
        }
    }
}
