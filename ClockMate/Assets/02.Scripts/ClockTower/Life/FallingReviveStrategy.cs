using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingReviveStrategy : IReviveStrategy
{
    private FallingAttack fallingAttack;

    public FallingReviveStrategy(FallingAttack attackRef)
    {
        fallingAttack = attackRef;
    }

    public Vector3 GetRevivePosition()
    {
        return fallingAttack.GetRandomSpawnPos(1f);
    }
}
