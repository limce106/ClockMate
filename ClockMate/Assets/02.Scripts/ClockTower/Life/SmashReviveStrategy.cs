using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashReviveStrategy : IReviveStrategy
{
    private SmashAttack smashAttack;

    public SmashReviveStrategy(SmashAttack attackRef)
    {
        smashAttack = attackRef;
    }

    public Vector3 GetRevivePosition()
    {
        return smashAttack.GetRandomRevivePos();
    }
}
