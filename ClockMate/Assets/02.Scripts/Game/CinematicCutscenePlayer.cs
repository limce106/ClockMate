using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static Define.Map;

public class CinematicCutscenePlayer : MonoBehaviour
{
    public PlayableDirector director;
    public float maxExtraTimeout = 1f;

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
        if (director != null && director.state == PlayState.Playing)
        {
            director.Stop();
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