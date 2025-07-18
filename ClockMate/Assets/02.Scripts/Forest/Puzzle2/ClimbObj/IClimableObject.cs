using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClimableObject : IInteractable
{
    void AttachTo(CharacterBase character);
}
