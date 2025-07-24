using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� �帧 ����
/// </summary>
public class BattleManager : MonoBehaviourPun
{
    [SerializeField] private List<GameObject> attackPrefabs;
    private int curAttackIdx = 0;

    [Header("UI")]
    public Slider recoverySlider;

    [Tooltip("�ν����Ϳ��� �� �������� �� ��")]
    public int round = 1;

    public const float recoveryPerSuccess = 0.334f;

    void Start()
    {
        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        while(recoverySlider.value < 1f)
        {
            yield return StartCoroutine(RunSinglePhase());
        }

        // TODO ���� ����
    }

    private IEnumerator RunSinglePhase()
    {
        curAttackIdx = 0;

        while (curAttackIdx < attackPrefabs.Count)
        {
            GameObject attackGO = PhotonNetwork.Instantiate(attackPrefabs[curAttackIdx].name, Vector3.zero, Quaternion.identity);
            AttackPattern curAttack = attackGO.GetComponent<AttackPattern>();

            yield return StartCoroutine(curAttack.Run());
            // ���� �Ϸ� �� ��� �ð�
            yield return new WaitForSeconds(1f);

            PhotonNetwork.Destroy(attackGO);

            if (curAttack.IsSuccess())
            {
                if (curAttack is PlayerAttackPattern)
                {
                    UpdateRecovery();
                }

                curAttackIdx++;
            }
            else
            {
                // TODO ���� ����

                // �÷��̾� �ݰ��� �����ߴٸ� ù ������� ���ư�
                if (curAttack is PlayerAttackPattern)
                {
                    curAttackIdx = 0;
                }
                // ���� ��� ���� ��(�� �÷��̾� ��� ��� ��) �ش� ��� �����
            }
        }

        round++;
    }

    private void UpdateRecovery()
    {
        recoverySlider.value += recoveryPerSuccess;
    }
}
