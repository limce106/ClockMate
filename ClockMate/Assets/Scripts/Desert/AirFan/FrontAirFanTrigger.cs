using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class FrontAirFanTrigger : MonoBehaviour
{
    private const float sizeYOffset = 1f;
    private const float centerYOffset = 0.5f;
    private BoxCollider frontBoxCollider;
    private AirFan airFan;

    private void Awake()
    {
        frontBoxCollider = GetComponent<BoxCollider>();
        airFan = GetComponentInParent<AirFan>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null )
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Milli;
        }

        if (characterIdentifier && isTargetCharacter)
        {
            StopCoroutine(airFan.LaunchPlayerParabola());
            airFan.SetMilliInTrigger(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player")
            return;

        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null)
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Milli;
        }

        if (characterIdentifier && isTargetCharacter)
        {
            if (!airFan.isUpwardFly && airFan.isFanOn && !AirFan.isFlying)
            {
                StartCoroutine(airFan.LaunchPlayerParabola());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
            return;

        var characterIdentifier = other.transform.root.GetComponent<PlayerIdentifier>();
        bool isTargetCharacter = false;
        if (characterIdentifier != null)
        {
            isTargetCharacter = characterIdentifier.characterId == Define.Character.CharacterId.Milli;
        }

        if (characterIdentifier && isTargetCharacter)
        {
            airFan.SetMilliInTrigger(false);
        }
    }

    public bool IsPlayerInXZRange(Vector3 playerPos)
    {
        Vector3 localPoint = frontBoxCollider.transform.InverseTransformPoint(playerPos);
        Vector3 halfSize = frontBoxCollider.size * 0.5f;

        bool inXZRange = Mathf.Abs(localPoint.x) <= halfSize.x &&
                         Mathf.Abs(localPoint.z) <= halfSize.z;

        return inXZRange;
    }

    public void ExpandFrontTrigger()
    {
        frontBoxCollider.size = new Vector3(frontBoxCollider.size.x, frontBoxCollider.size.y + sizeYOffset, frontBoxCollider.size.z);
        frontBoxCollider.center = new Vector3(frontBoxCollider.center.x, 1 + (frontBoxCollider.size.y - 1) * centerYOffset, frontBoxCollider.center.z);
    }
}
