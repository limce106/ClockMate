using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define.Battle;

/// <summary>
/// ���� �帧 ����
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<GameObject> attackPrefabs;
    private int curAttackIdx = 0;

    [Header("UI")]
    public Slider recoverySlider;

    [Tooltip("�ν����Ϳ��� �� �������� �� ��")]
    public int round = 1;

    public const float recoveryPerSuccess = 0.334f;

    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartBattle());
    }

    //public override void OnJoinedRoom()
    //{
    //    StartCoroutine(StartBattle());
    //}

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
            StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        StartCoroutine(RunSinglePhase());

        // TODO ���� ����
    }

    private IEnumerator RunSinglePhase()
    {
        curAttackIdx = 0;

        while (curAttackIdx < attackPrefabs.Count)
        {
            string attackName = attackPrefabs[curAttackIdx].name.Trim();
            GameObject attackGO = PhotonNetwork.Instantiate("Prefabs/" + attackName, Vector3.zero, Quaternion.identity);
            AttackPattern curAttack = attackGO.GetComponent<AttackPattern>();

            yield return StartCoroutine(curAttack.Run());
            // ���� �Ϸ� �� ��� �ð�
            yield return new WaitForSeconds(1f);

            PhotonNetwork.Destroy(attackGO);

            if (curAttack.IsSuccess())
            {
                if (curAttack.attackCharacter == AttackCharacter.Player)
                {
                    UpdateRecovery();
                }

                curAttackIdx++;
            }
            else
            {
                // TODO ���� ����

                // �÷��̾� �ݰ��� �����ߴٸ� ù ������� ���ư�
                if (curAttack.attackCharacter == AttackCharacter.Player)
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
