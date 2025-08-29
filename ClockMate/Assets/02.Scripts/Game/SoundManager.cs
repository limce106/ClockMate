using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType { BGM, Effect }

public struct SoundHandle
{
    internal int Id;
    public bool IsValid => Id != 0;
}

[RequireComponent(typeof(PhotonView))]
public class SoundManager : MonoPunSingleton<SoundManager>
{
    [Header("Audio Assets")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip[] audioClipList;

    [Header("BGM")]
    [SerializeField] private float defaultBgmFade = 0.75f;

    [Header("Network")]
    [SerializeField] private double netLeadTime = 0.15; // 네트워크 헤드룸(초)
    
    // 상태
    [SerializeField] private string currentBGM;

    // 내부 캐시
    private readonly Dictionary<string, AudioClip> _clipMap = new();
    private readonly Dictionary<SoundType, AudioMixerGroup> _groupMap = new();

    // SFX 풀/인스턴스 관리
    private readonly Stack<SoundPlayer> _pool = new();
    private readonly Dictionary<int, SoundPlayer> _activeById = new();
    private readonly Dictionary<string, HashSet<int>> _activeByKey = new();

    private int _idSeed = 1;
    private Transform _sfxRoot;

    // BGM 전용 소스(크로스페이드용 2개)
    private AudioSource _bgmA, _bgmB;
    private bool _bgmToggle;
    
    private PhotonView _pv;

    protected override void Init()
    {
        base.Init();
        
        // 클립 로드(중복 방지)
        foreach (AudioClip clip in audioClipList)
        {
            if (clip == null) continue;
            if (!_clipMap.TryAdd(clip.name, clip))
            {
                Debug.LogWarning($"[SoundManager] Duplicate clip name: {clip.name})");
            }
        }

        // 믹서 그룹 캐시
        CacheMixerGroup(SoundType.BGM);
        CacheMixerGroup(SoundType.Effect);

        // SFX Root
        _sfxRoot = new GameObject("SFX_Root").transform;
        _sfxRoot.SetParent(transform);

        // BGM 소스 2개
        _bgmA = gameObject.AddComponent<AudioSource>();
        _bgmB = gameObject.AddComponent<AudioSource>();
        foreach (AudioSource source in new[] { _bgmA, _bgmB })
        {
            source.loop = true;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = GetGroup(SoundType.BGM);
            source.spatialBlend = 0f;
            source.volume = 0f;
        }
        _pv = GetComponent<PhotonView>();
    }

    private void CacheMixerGroup(SoundType type)
    {
        AudioMixerGroup[] groups = audioMixer != null ? audioMixer.FindMatchingGroups(type.ToString()) : null;
        if (groups == null || groups.Length == 0)
        {
            Debug.LogWarning($"[SoundManager] Mixer group not found for {type}.");
            _groupMap[type] = null;
        }
        else _groupMap[type] = groups[0];
    }

    private AudioMixerGroup GetGroup(SoundType type) => _groupMap.GetValueOrDefault(type);

    public string CurrentBGM => currentBGM;

    /// <summary>
    /// SFX 재생(핸들 반환). pos=null이면 2D, 값 있으면 3D.
    /// 동기화는 false가 디폴트.
    /// </summary>
    public SoundHandle PlaySfx(
        string key,
        bool loop = false,
        Vector3? pos = null,
        float volume = 1f,
        float pitch = 1f,
        float delay = 0f,
        bool sync = false,
        bool includeSelf = true)
    {
        if (!sync)
        {
            return PlaySfx_Local(key, loop, pos, volume, pitch, delay);
        }
        
        // 네트워크 기준 시작 시각(모든 클라 공통)
        double start = PhotonNetwork.Time + netLeadTime;

        // 로컬도 재생(동일 기준 시간으로 딜레이)
        if (includeSelf)
        {
            float localDelay = Mathf.Max(0f, (float)(start - PhotonNetwork.Time));
            PlaySfx_Local(key, loop, pos, volume, pitch, localDelay);
        }

        // 원격에게 알림
        bool spatial = pos.HasValue;
        Vector3 p = spatial ? pos.Value : Vector3.zero;
        _pv.RPC(nameof(RPC_PlaySfx), RpcTarget.Others,
            key, loop, spatial, p, volume, pitch, start);

        return default;
    }

    private SoundHandle PlaySfx_Local(string key, bool loop, Vector3? pos, float volume, float pitch, float delay)
    {
        AudioClip clip = GetClipOrWarn(key);
        if (!clip) return default;

        SoundPlayer player = RentPlayer();
        player.Configure(
            clip: clip,
            group: GetGroup(SoundType.Effect),
            loop: loop,
            volume: volume,
            pitch: pitch,
            spatial: pos.HasValue,
            worldPos: pos);

        SoundHandle handle = RegisterActive(key, player);
        player.Play(delay, OnPlayerFinish);
        return handle;
    }

    [PunRPC]
    private void RPC_PlaySfx(string key, bool loop, bool spatial, Vector3 pos,
        float volume, float pitch, double netStart)
    {
        float delay = Mathf.Max(0f, (float)(netStart - PhotonNetwork.Time));
        PlaySfx_Local(key, loop, spatial ? (Vector3?)pos : null, volume, pitch, delay);
    }
    
    /// <summary>
    /// BGM 전환(크로스페이드).
    /// </summary>
    public void PlayBgm(string key, float fade = -1f)
    {
        AudioClip clip = GetClipOrWarn(key);
        if (clip == null) return;

        if (fade < 0f) fade = defaultBgmFade;

        AudioSource dst = _bgmToggle ? _bgmA : _bgmB;
        AudioSource src = _bgmToggle ? _bgmB : _bgmA;
        _bgmToggle = !_bgmToggle;

        dst.clip = clip;
        dst.outputAudioMixerGroup = GetGroup(SoundType.BGM);
        dst.volume = 0f;
        dst.Play();

        StopAllCoroutines();
        StartCoroutine(CrossFade(src, dst, fade));

        currentBGM = key;
    }

    /// <summary>
    /// 개별 핸들 정지
    /// </summary>
    public void Stop(SoundHandle handle)
    {
        if (!handle.IsValid) return;
        if (_activeById.TryGetValue(handle.Id, out SoundPlayer player) && player != null)
            player.StopImmediate();
    }

    /// <summary>
    /// 같은 키(클립 이름)로 재생 중인 SFX 모두 정지
    /// </summary>
    public void StopByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!_activeByKey.TryGetValue(key, out HashSet<int> set)) return;
        
        var tmp = new List<int>(set);
        foreach (int id in tmp)
        {
            if (_activeById.TryGetValue(id, out SoundPlayer player) && player != null)
                player.StopImmediate();
        }
    }
    
    /// <summary>
    /// 동기화 여부를 지정하여 정지
    /// </summary>
    public void StopByKey(string key, bool sync)
    {
        if (sync) _pv.RPC(nameof(RPC_StopByKey), RpcTarget.Others, key);
        StopByKey(key);
    }
    
    [PunRPC]
    private void RPC_StopByKey(string key) => StopByKey(key);

    /// <summary>
    /// 전체 정지(옵션: 타입 제한)
    /// </summary>
    public void StopAll(SoundType? type = null)
    {
        if (type is null or SoundType.Effect)
        {
            var tmp = new List<int>(_activeById.Keys);
            foreach (int id in tmp)
            {
                if (_activeById.TryGetValue(id, out SoundPlayer p) && p != null)
                    p.StopImmediate();
            }
        }

        if (type is null or SoundType.BGM)
        {
            _bgmA.Stop(); _bgmA.clip = null; _bgmA.volume = 0f;
            _bgmB.Stop(); _bgmB.clip = null; _bgmB.volume = 0f;
            currentBGM = null;
        }
    }

    /// <summary>
    /// BGM 볼륨(0~1) 세팅
    /// </summary>
    public void SetVolume(float volume01)
    {
        volume01 = Mathf.Clamp01(volume01);
        _bgmA.volume = volume01;
        _bgmB.volume = volume01;
    }

    private AudioClip GetClipOrWarn(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[SoundManager] key is null or empty.");
            return null;
        }
        if (_clipMap.TryGetValue(key, out AudioClip clip) && clip) return clip;
        
        Debug.LogWarning($"[SoundManager] Clip not found: {key}");
        return null;
    }

    private SoundPlayer RentPlayer()
    {
        SoundPlayer player = _pool.Count > 0 ? _pool.Pop() : CreateNewPlayer();
        player.gameObject.SetActive(true);
        return player;
    }

    private void ReturnPlayer(SoundPlayer player)
    {
        if (!player) return;
        player.ResetForPool();
        player.transform.SetParent(_sfxRoot, false);
        player.gameObject.SetActive(false);
        _pool.Push(player);
    }

    private SoundPlayer CreateNewPlayer()
    {
        var go = new GameObject("SFX_Player");
        go.transform.SetParent(_sfxRoot, false);
        var p = go.AddComponent<SoundPlayer>();
        go.SetActive(false);
        return p;
    }

    private SoundHandle RegisterActive(string key, SoundPlayer player)
    {
        int id = ++_idSeed;
        _activeById[id] = player;
        if (!_activeByKey.TryGetValue(key, out HashSet<int> set))
        {
            set = new HashSet<int>();
            _activeByKey[key] = set;
        }
        set.Add(id);

        // 핸들을 GameObject 이름에 표시(디버그 편의)
        player.name = $"SFX_Player_{key}_{id}";
        return new SoundHandle { Id = id };
    }

    private void OnPlayerFinish(SoundPlayer player)
    {
        if (!player) return;

        // id/key 역탐색
        int removeId = 0;

        foreach (var kv in _activeById)
        {
            if (kv.Value == player)
            {
                removeId = kv.Key;
                break;
            }
        }
        if (removeId != 0)
        {
            foreach (var kv in _activeByKey)
            {
                if (kv.Value.Contains(removeId))
                {
                    kv.Value.Remove(removeId);
                    if (kv.Value.Count == 0) _activeByKey.Remove(kv.Key);
                    break;
                }
            }
            _activeById.Remove(removeId);
        }

        // 풀로 복귀
        ReturnPlayer(player);
    }

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float duration)
    {
        float timer = 0f;
        float fromStart = from.clip ? from.volume : 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float k = duration <= 0f ? 1f : timer / duration;
            to.volume = Mathf.Lerp(0f, 1f, k);
            if (from.clip) from.volume = Mathf.Lerp(fromStart, 0f, k);
            yield return null;
        }
        to.volume = 1f;
        if (from.clip)
        {
            from.Stop(); from.clip = null; from.volume = 0f;
        }
    }
}