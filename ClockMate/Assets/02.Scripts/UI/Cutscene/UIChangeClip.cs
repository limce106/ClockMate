using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class UIChangeClip : PlayableAsset, ITimelineClipAsset
{
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("Override Positions?")]
    public bool overridePosA;
    public bool overridePosB;

    public Vector2 posA;
    public Vector2 posB;

    [TextArea] public string text;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<UIChangeBehaviour>.Create(graph);
        var b = playable.GetBehaviour();

        b.spriteA = spriteA;
        b.spriteB = spriteB;

        b.overridePosA = overridePosA;
        b.overridePosB = overridePosB;
        b.posA = posA;
        b.posB = posB;

        b.text = text;

        return playable;
    }
}