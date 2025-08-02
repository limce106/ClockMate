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

    /// <summary>
    /// �ʵ� �ʱ�ȭ
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// ���� ��� ����
    /// </summary>
    public abstract IEnumerator Run();

    /// <summary>
    /// �÷��̾� ��� ����� ��츦 ���� �ߵ� ���� ��� + ������Ʈ ����
    /// </summary>
    public virtual void CancelAttack()
    {
        
    }
}
