using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 로컬 비디오 재생기.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoCutscenePlayer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage targetImage;
    [SerializeField] private RenderTexture renderTexture;
    
    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    
    private Action _onFinished; // 재생 완료 콜백
    private bool _playing; // 재생 중 여부

    private void Awake()
    {
        if (!videoPlayer) videoPlayer = gameObject.AddComponent<VideoPlayer>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.isLooping = false;

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnLoopPointReached;

        if (targetImage) targetImage.enabled = false; // 시작 전에는 화면에 보이지 않게
        if (renderTexture) renderTexture.Release(); // 이전 프레임 흔적 초기화
    }

    public void PlayClip(string clipName, Action onFinished)
    {
        VideoClip clip = Resources.Load<VideoClip>($"Cutscenes/{clipName}");
        if (!clip) { Debug.LogError("[CutscenePlayer] clip null"); onFinished?.Invoke(); return; }
        if (_playing) { Debug.LogWarning("[CutscenePlayer] already playing"); onFinished?.Invoke(); return; }

        _onFinished = onFinished;
        _playing = true;

        videoPlayer.clip = clip;
        if (renderTexture) renderTexture.Release();
        videoPlayer.Prepare();
    }

    public void Skip()
    {
        if (!_playing) return;
        videoPlayer.Stop();
        SafeFinish();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        if (targetImage)
        {
            targetImage.texture = renderTexture;
            targetImage.enabled = true;
        }
        vp.Play();
        Debug.Log($"[VideoCutscenePlayer] Play");
    }

    private void OnLoopPointReached(VideoPlayer vp)
    {
        SafeFinish();
    }

    private void SafeFinish()
    {
        if (!_playing) return;
        _playing = false;

        if (targetImage) targetImage.enabled = false;
        if (renderTexture) renderTexture.Release();

        Action cb = _onFinished;
        _onFinished = null;

        cb?.Invoke();
    }

    void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached  -= OnLoopPointReached;
    }
}
