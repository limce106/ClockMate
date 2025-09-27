using DefineExtension;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class LadderClimbObject : ClimbObjectBase
{
    public override void AttachTo(CharacterBase character)
    {
        // 사다리 밑에 있으면
        if(character.transform.position.y < transform.position.y)
        {
            character.transform.position = bottomPoint.position;
            character.Anim.SetClimbUp(true);
        }
        else
        {
            character.transform.position = topPoint.position;
            character.Anim.SetClimbDown(true);
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        character.transform.forward = forward.normalized;
    }
}
