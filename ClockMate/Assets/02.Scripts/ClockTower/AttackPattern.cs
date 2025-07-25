using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

/// <summary>
/// ���� ���ݰ� �÷��̾� �ݰ� ���� ����
/// </summary>
[RequireComponent(typeof(PhotonView))]
public abstract class AttackPattern : MonoBehaviourPun
{
    public AttackCharacter attackCharacter;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        SpawnObj();
        StartCoroutine(Run());
    }

    /// <summary>
    /// �ʵ� �ʱ�ȭ
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// ��� ������Ʈ ����
    /// </summary>
    protected abstract void SpawnObj();

    /// <summary>
    /// ���� ��� ����
    /// </summary>
    public abstract IEnumerator Run();

    /// <summary>
    /// �÷��̾ ���� ����� �����ߴ���
    /// </summary>
    public abstract bool IsSuccess();
}
