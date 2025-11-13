using Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

public class LDPlayerAttackQuest : LocalDataBase
{
    public PlayerAttackType PlayerAttackType { get; set; }
    public string HourQuest { get; set; }
    public string MilliQuest { get; set; }
}
