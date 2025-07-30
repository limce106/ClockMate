using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultReviveStrategy : IReviveStrategy
{
    private Vector3 position;

    public DefaultReviveStrategy(Vector3 pos)
    {
        position = pos;
    }
    public Vector3 GetRevivePosition()
    {
        return position;
    }
}
