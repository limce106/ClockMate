using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

/// <summary>
/// 보스 공격과 플레이어 반격 공통 구조
/// </summary>
[RequireComponent(typeof(PhotonView))]
public abstract class AttackPattern : MonoBehaviourPun
{
    public AttackCharacter attackCharacter;
    public bool cleanUpEnded { get; protected set; } = false; // 해당 공격이 생성한 오브젝트 제거를 모두 완료했는지 여부

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 필드 초기화
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 공격 기믹 실행
    /// </summary>
    public abstract IEnumerator Run();

    /// <summary>
    /// 공격이 중단/종료된 후 관련 오브젝트, UI 제거, 플레이어 부착 해제 등 실행
    /// </summary>
    public abstract void CleanUpAttack();
}
