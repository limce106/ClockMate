using UnityEngine;
using UnityEngine.Playables;

public class UIChangeBehaviour : PlayableBehaviour
{
    [HideInInspector] public Sprite spriteA;
    [HideInInspector] public Sprite spriteB;

    [HideInInspector] public bool overridePosA;
    [HideInInspector] public bool overridePosB;
    [HideInInspector] public Vector2 posA;
    [HideInInspector] public Vector2 posB;

    [HideInInspector] public string text;
}