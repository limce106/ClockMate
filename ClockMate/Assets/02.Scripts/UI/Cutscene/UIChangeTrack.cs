using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.8f, 1f)]
[TrackClipType(typeof(UIChangeClip))]
[TrackBindingType(typeof(TimelineUIBinder))]
public class UIChangeTrack : TrackAsset
{
    [Header("Default Positions")]
    public Vector2 singlePosA;   // 이미지 1개 A 위치
    public Vector2 doublePosA;   // 이미지 2개 A 위치
    public Vector2 doublePosB;   // 이미지 2개 B 위치

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<UIChangeMixerBehaviour>.Create(graph, inputCount);
        playable.GetBehaviour().track = this;   // 믹서가 트랙 기본값을 읽게 연결
        return playable;
    }
}