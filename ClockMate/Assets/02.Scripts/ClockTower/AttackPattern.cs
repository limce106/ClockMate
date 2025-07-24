using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 공격과 플레이어 반격 공통 구조
/// </summary>
public abstract class AttackPattern : MonoBehaviour
{
    protected float duration;  // 기믹 타임
    protected bool isSuccess;   // 플레이어가 현재 기믹을 성공했는지

    private void Start()
    {
        SpawnObj();
        StartCoroutine(Run());
    }

    /// <summary>
    /// 기믹 오브젝트 스폰
    /// </summary>
    public abstract void SpawnObj();
    /// <summary>
    /// 기믹 실행
    /// </summary>
    public abstract IEnumerator Run();
    /// <summary>
    /// 플레이어가 현재 기믹을 성공했는지
    /// </summary>
    public bool IsSuccess() => isSuccess;
}
