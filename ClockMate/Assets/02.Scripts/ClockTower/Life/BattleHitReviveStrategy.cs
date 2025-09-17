using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHitReviveStrategy : IReviveStrategy
{
    private Vector3 lastPosition;

    public BattleHitReviveStrategy(Vector3 hitPos)
    {
        lastPosition = hitPos;
    }

    public Vector3 GetRevivePosition()
    {
        return lastPosition;
    }
}
