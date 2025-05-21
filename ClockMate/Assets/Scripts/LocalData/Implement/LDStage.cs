using System.Collections.Generic;
using Define;
using UnityEngine;
using static Define.Character;

public class LDStage: LocalDataBase
{
    public Map.MapName Map { get; set; }
    public int ReviveCount { get; set; }
    public List<float> HourLoadPos { get; set; }
    public List<float> MilliLoadPos { get; set; }
    public int NextStageID { get; set; }
}