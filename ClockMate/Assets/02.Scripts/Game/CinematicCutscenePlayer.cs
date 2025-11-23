using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using static Define.Map;

public class CinematicCutscenePlayer : MonoBehaviour
{
    public PlayableDirector director;
    public float maxExtraTimeout = 1f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TimelineUIBinder subtitleSource;
    [SerializeField] private GameObject subtitleRoot;
    //private string _audioTrackName = "Audio Track";
    //private string _subtitleTrackName = "Subtitle Track";
    public event Action OnFinished;
    private bool _prepared = false;

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        foreach (PuzzleMapName puzzleMap in Enum.GetValues(typeof(PuzzleMapName)))
        {
            if(SceneManager.GetActiveScene().name.Equals(puzzleMap.ToString()))
            {
                // 씬에 배치된 디렉터 자동 연결
                if (director == null)
                {
                    director = FindObjectOfType<PlayableDirector>(includeInactive: true);
                    if (director == null)
                    {
                        Debug.LogError("[CinematicCutscenePlayer] No Playable Director found");
                    }
                }
                return;
            }
        }
        
    }
    public void Prepare(string clipName)
    {
        var timeline = Resources.Load<PlayableAsset>($"Cutscenes/Timelines/{clipName}");
        if (timeline == null)
        {
            Debug.LogError($"[CinematicCutscenePlayer] Timeline not found: Cutscenes/Timelines/{clipName}");
            _prepared = false;
            return;
        }
        
        // audio source, subtitle source 바인딩
        var timelineAsset = timeline as TimelineAsset;
        if (timelineAsset != null)
        {
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track is AudioTrack)
                {
                    director.SetGenericBinding(track, audioSource);
                } else if (track is UIChangeTrack)
                {
                    director.SetGenericBinding(track, subtitleSource);
                    subtitleRoot.SetActive(true);
                }
                
            }
        }

        director.playableAsset = timeline;
        director.initialTime = 0;
        director.stopped -= DirectorOnStopped;
        director.stopped += DirectorOnStopped;
        _prepared = true;
    }

    public void Play()
    {
        if (!_prepared)
        {
            Debug.LogError("[CinematicCutscenePlayer] Not prepared.");
            OnFinished?.Invoke();
            return;
        }

        director.Play();

        // 강제 타임아웃
        CancelInvoke(nameof(ForceStop));
        float duration = (float)director.playableAsset.duration;
        Invoke(nameof(ForceStop), duration + maxExtraTimeout);
    }

    public void Stop()
    {
        if (director == null) return;
        if (director.playableAsset != null)
        {
            // 끝 프레임까지 이동
            director.time = director.duration + 0.001f;
            director.Evaluate();
        }

        if (director.state == PlayState.Playing)
        {
            director.Stop();
        }

        if (subtitleRoot.activeSelf)
        {
            subtitleRoot.SetActive(false);
        }

        CancelInvoke(nameof(ForceStop));
        OnFinished?.Invoke();
    }

    private void DirectorOnStopped(PlayableDirector obj)
    {
        // 디렉터 정지 시 호출
        CancelInvoke(nameof(ForceStop));
        OnFinished?.Invoke();
    }

    private void ForceStop()
    {
        Debug.LogWarning("[CinematicCutscenePlayer] ForceStop due to timeout.");
        Stop();
    }
}