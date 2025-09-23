using Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDLoadingPosition : LocalDataBase
{
    public Map.MapName Map { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
}
