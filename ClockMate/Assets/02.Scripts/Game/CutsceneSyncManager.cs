using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// 컷신 동기화 유틸
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class CutsceneSyncManager : MonoPunSingleton<CutsceneSyncManager>
{
    [SerializeField] private VideoCutscenePlayer cutscenePlayer;
    [SerializeField] private CinematicCutscenePlayer cinematicPlayer;
    
    // 컷신 타입
    private enum CutsceneType { Video = 0, Cinematic = 1 }
    private CutsceneType _currentType = CutsceneType.Video;
    
    // 진행 상태
    public bool IsBusy { get; private set; } 
    private int  _seq; // 컷신마다 증가하는 식별자 번호
    private int  _currentId = -1; // 현재 실행 중인 컷신의 식별자

    // 마스터만 사용(집계/타임아웃)
    private HashSet<int> _expectedActors = new(); // 기대 인원
    private HashSet<int> _finishedActors = new(); // 완료 인원
    private float _timeoutSec; // 타임아웃 시간
    private float _deadline; // 타임아웃 만료 절대 시각
    
    // 마스터 전용 후처리
    private Action _masterOnlyOnAllFinished;
    
    void Update()
    {
        // 마스터: 타임아웃 감시
        if (!IsBusy || !PhotonNetwork.IsMasterClient) return;
        if (Time.realtimeSinceStartup >= _deadline)
        {
            Debug.LogWarning($"[CutsceneSync] Timeout. Force finish. id={_currentId}");
            photonView.RPC(nameof(RPC_AllFinished), RpcTarget.All, _currentId); // 종료 브로드캐스트
            _masterOnlyOnAllFinished?.Invoke(); // 마스터 전용 후처리 실행
        }
    }

    /// <summary>
    /// 마스터 전용, 모든 클라 대상 컷신 시작
    /// </summary>
    public void PlayForAll(
        string clipName,
        float timeoutSec = 0f,
        Action masterOnlyOnAllFinished = null)
    {
        PlayForAll_Internal(clipName, CutsceneType.Video, timeoutSec, masterOnlyOnAllFinished);
    }
    
    /// <summary>
    /// 마스터 전용, 모든 클라 대상 시네마틱 컷신 시작
    /// </summary>
    public void PlayCinematicForAll(
        string cutsceneName,
        float timeoutSec = 0f,
        Action masterOnlyOnAllFinished = null)
    {
        PlayForAll_Internal(cutsceneName, CutsceneType.Cinematic, timeoutSec, masterOnlyOnAllFinished);
    }
    
    private void PlayForAll_Internal(
        string cutsceneName,
        CutsceneType type,
        float timeoutSec,
        Action masterOnlyOnAllFinished)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[CutsceneSync] PlayForAll: Only Master can start.");
            return;
        }
        if (IsBusy)
        {
            Debug.LogWarning("[CutsceneSync] Already running.");
            return;
        }
        if (string.IsNullOrEmpty(cutsceneName))
        {
            Debug.LogError("[CutsceneSync] cutsceneName null/empty.");
            return;
        }
        
        // 상태 초기화
        IsBusy = true;
        _currentId = ++_seq;

        _expectedActors.Clear();
        _finishedActors.Clear();
        foreach (var p in PhotonNetwork.CurrentRoom.Players)
            _expectedActors.Add(p.Value.ActorNumber);

        _timeoutSec = Mathf.Max(0f, timeoutSec);
        _deadline   = _timeoutSec > 0f ? Time.realtimeSinceStartup + _timeoutSec : float.PositiveInfinity;

        _masterOnlyOnAllFinished = masterOnlyOnAllFinished;

        // 타입 전달
        photonView.RPC(nameof(RPC_Begin), RpcTarget.All, cutsceneName, _currentId, (int)type);
        Debug.Log($"[CutsceneSync] Begin: id={cutsceneName}, type={type}, cutsceneId={_currentId}, expect={_expectedActors.Count}");
    }


    // 로컬 재생 완료 시 호출(등록된 startLocal 내부에서 onLocalFinished로 전달)
    private void NotifyLocalFinished()
    {
        if (_currentId < 0) return;
        photonView.RPC(nameof(RPC_ReportFinished), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, _currentId);
    }


    #region RPC

    [PunRPC]
    private void RPC_Begin(string clipName, int cutsceneId, int typeInt)
    {
        // 동시 재생 중복 방지
        if (IsBusy && _currentId != cutsceneId)
        {
            Debug.LogWarning($"[CutsceneSync] Begin ignored. Local busy id={_currentId} incoming={cutsceneId}");
            return;
        }

        IsBusy = true;
        _currentId = cutsceneId;

        _currentType = (CutsceneType)typeInt;

        // 입력 잠금
        GameManager.Instance.SetLocalCharacterInput(false);

        // 타입에 따라 분기
        if (_currentType == CutsceneType.Video)
        {
            // BGM 끄기
            SoundManager.Instance.StopAll(SoundType.BGM);
            SoundManager.Instance.StopAll(SoundType.Effect);
            cutscenePlayer.PlayClip(clipName, NotifyLocalFinished);
        }
        else // Cinematic
        {
            if (cinematicPlayer == null)
            {
                Debug.LogError("[CutsceneSync] Cinematic player not assigned.");
                // 즉시 종료 보고
                NotifyLocalFinished();
                return;
            }

            // 중복 구독 방지
            cinematicPlayer.OnFinished -= OnCinematicFinished;
            cinematicPlayer.OnFinished += OnCinematicFinished;

            cinematicPlayer.Prepare(clipName); // Resources/Cutscenes/Timelines/{id}.playable 로드
            cinematicPlayer.Play();
        }
    }


    [PunRPC]
    private void RPC_ReportFinished(int actorNumber, int cutsceneId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!IsBusy || _currentId != cutsceneId) return; // 상태 & 컷신 식별자 확인

        _finishedActors.Add(actorNumber); // 완료 저장
        TryConcludeByMaster(); // 종료 조건 만족 여부 확인
    }

    [PunRPC]
    private void RPC_AllFinished(int cutsceneId)
    {
        if (!IsBusy || _currentId != cutsceneId) return; // 상태 & 컷신 식별자 확인

        if (_currentType == CutsceneType.Video)
        {
            cutscenePlayer.Skip();
            GameManager.Instance.PlayMapBgm();
        }
        else
        {
            if (cinematicPlayer != null)
            {
                cinematicPlayer.OnFinished -= OnCinematicFinished; // 구독 해제
                cinematicPlayer.Stop(); // 재생 중이면 정지 + 카메라 그룹 비활성 등 처리
            }
        }
        
        ResetState(); // 상태 리셋
        GameManager.Instance.SetLocalCharacterInput(true);
    }
    
    #endregion

    /// <summary>
    /// 종료 집계 후 처리
    /// </summary>
    private void TryConcludeByMaster()
    {
        if (!IsBusy || !PhotonNetwork.IsMasterClient) return;
        
        // 기대 인원이 0이거나 모든 기대 인원이 모두 완료하였다면 종료
        if (_expectedActors.Count == 0 || _finishedActors.IsSupersetOf(_expectedActors))
        {
            _masterOnlyOnAllFinished?.Invoke(); // 마스터 전용 후처리 실행
            photonView.RPC(nameof(RPC_AllFinished), RpcTarget.All, _currentId); // 종료 브로드캐스트
        }
    }
    
    private void ResetState()
    {
        IsBusy = false;
        _currentId = -1;
        _timeoutSec = 0f;
        _deadline = 0f;
        _expectedActors.Clear();
        _finishedActors.Clear();
        _masterOnlyOnAllFinished = null;
    }
    
    private void OnCinematicFinished()
    {
        NotifyLocalFinished();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient || !IsBusy) return;

        if (_expectedActors.Remove(otherPlayer.ActorNumber))
            TryConcludeByMaster();
    }
}
