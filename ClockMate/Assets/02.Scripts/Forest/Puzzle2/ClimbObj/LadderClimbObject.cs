using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class LadderClimbObject : ClimbObjectBase
{
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;

    private bool _isTrigger = false;
    private bool _isColliding = false;

    void Start()
    {
        topY = topPoint.position.y;
        bottomY = bottomPoint.position.y;
    }

    public override void AttachTo(CharacterBase character)
    {
        // 사다리 밑에 있으면
        if(_isColliding)
        {
            character.transform.position = bottomPoint.position;
        }
        else
        {
            character.transform.position = topPoint.position;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        character.transform.forward = forward.normalized;
    }

    public override bool CanInteract(CharacterBase character)
    {
        if (!base.CanInteract(character))
            return false;

        // 사다리 밑에 있을 때
        if(character.transform.position.y < gameObject.transform.position.y)
        {
            if (_isColliding)
                return true;
            else
                return false;
        }
        else
        {
            if (_isTrigger)
                return true;
            else
                return false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        _isTrigger = true;
    }
    private void OnTriggerExit(Collider other)
    {
        _isTrigger = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isColliding = false;
    }
}
