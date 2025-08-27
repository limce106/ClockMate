using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    private AudioSource _src;
    private System.Action<SoundPlayer> _onFinish; // 매니저에 반환/정리 콜백

    public string ClipName => _src.clip != null ? _src.clip.name : null;
    public bool IsLoop => _src.loop;

    void Awake() => _src = GetComponent<AudioSource>();

    public void Configure(
        AudioClip clip,
        AudioMixerGroup group,
        bool loop,
        float volume = 1f,
        float pitch = 1f,
        bool spatial = false,
        Vector3? worldPos = null)
    {
        _src.clip = clip;
        _src.outputAudioMixerGroup = group;
        _src.loop = loop;
        _src.volume = Mathf.Clamp01(volume);
        _src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        _src.spatialBlend = spatial ? 1f : 0f;
        if (worldPos.HasValue) transform.position = worldPos.Value;
    }

    public void Play(float delay = 0f, System.Action<SoundPlayer> onFinish = null)
    {
        _onFinish = onFinish;
        if (_src.clip == null)
        {
            Debug.LogWarning("[SoundPlayer] Clip is null.");
            _onFinish?.Invoke(this);
            return;
        }

        if (_src.loop == false)
        {
            // 클립 길이(+딜레이) 후 자동 반환
            StartCoroutine(DestroyAfter(_src.clip.length + delay));
        }

        if (delay <= 0f) _src.Play();
        else _src.PlayDelayed(delay);
    }

    public void StopImmediate()
    {
        _src.Stop();
        _onFinish?.Invoke(this);
    }

    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        _onFinish?.Invoke(this);
    }

    // 풀 복귀 시 초기화
    public void ResetForPool()
    {
        _onFinish = null;
        _src.Stop();
        _src.clip = null;
        _src.loop = false;
        _src.outputAudioMixerGroup = null;
        _src.volume = 1f;
        _src.pitch = 1f;
        _src.spatialBlend = 0f;
    }
}