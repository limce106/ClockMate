using Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDPuzzleQuest : LocalDataBase
{
    public Map.MapName Map { get; set; }
    public string QuestNameImgPath { get; set; }
    public string HourQuest { get; set; }
    public string MilliQuest { get; set; }
}
