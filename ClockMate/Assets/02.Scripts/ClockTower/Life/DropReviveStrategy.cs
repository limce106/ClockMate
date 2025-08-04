using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropReviveStrategy : IReviveStrategy
{
    private DropAttack dropAttack;

    public DropReviveStrategy(DropAttack attackRef)
    {
        dropAttack = attackRef;
    }

    public Vector3 GetRevivePosition()
    {
        return dropAttack.GetRandomSpawnPos(1f);
    }
}
