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
    /// 플레이어가 현재 기믹을 성공했는지
    /// </summary>
    public virtual bool IsSuccess()
    {
        // TODO 두 플레이어가 죽으면 false 처리
        return true;
    }
}
