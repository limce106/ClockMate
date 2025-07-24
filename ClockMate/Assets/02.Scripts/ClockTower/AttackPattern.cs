using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ���ݰ� �÷��̾� �ݰ� ���� ����
/// </summary>
public abstract class AttackPattern : MonoBehaviour
{
    protected float duration;  // ��� Ÿ��
    protected bool isSuccess;   // �÷��̾ ���� ����� �����ߴ���

    private void Start()
    {
        SpawnObj();
        StartCoroutine(Run());
    }

    /// <summary>
    /// ��� ������Ʈ ����
    /// </summary>
    public abstract void SpawnObj();
    /// <summary>
    /// ��� ����
    /// </summary>
    public abstract IEnumerator Run();
    /// <summary>
    /// �÷��̾ ���� ����� �����ߴ���
    /// </summary>
    public bool IsSuccess() => isSuccess;
}
