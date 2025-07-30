using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define.Battle;

/// <summary>
/// 전투 흐름 제어
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, AttackType> AttackNameToType;

    [SerializeField] private List<GameObject> attackPrefabs;
    private int curAttackIdx = 0;

    public AttackType attackType { get; private set; }
    public PlayerAttackType playerAttackType { get; private set; }
    public SmashAttack currentSmashAttack { get; private set; }

    [Header("UI")]
    public Slider recoverySlider;

    [Tooltip("인스펙터에서 값 변경하지 말 것")]
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

        // StageLifeManager 파괴
        if (StageLifeManager.Instance != null)
            Destroy(StageLifeManager.Instance);

        // AttackNameToType 초기화
        AttackNameToType = new Dictionary<string, AttackType>
        {
            {"SwingAttack", AttackType.SwingAttack },
            {"SmashAttack", AttackType.SmashAttack },
            {"PlayerAttack", AttackType.PlayerAttack },
        };
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

        // TODO 성공 연출
    }

    private IEnumerator RunSinglePhase()
    {
        curAttackIdx = 0;

        while (curAttackIdx < attackPrefabs.Count)
        {
            string attackName = attackPrefabs[curAttackIdx].name.Trim();
            photonView.RPC(nameof(SetAttackType), RpcTarget.All, AttackNameToType[attackName]);

            GameObject attackGO = PhotonNetwork.Instantiate("Prefabs/" + attackName, Vector3.zero, Quaternion.identity);
            AttackPattern curAttack = attackGO.GetComponent<AttackPattern>();
            
            currentSmashAttack = attackType == AttackType.SmashAttack ? curAttack as SmashAttack : null;

            yield return StartCoroutine(curAttack.Run());
            // 공격 완료 후 대기 시간
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
                // TODO 실패 연출

                // 플레이어 반격이 실패했다면 첫 기믹으로 돌아감
                if (curAttack.attackCharacter == AttackCharacter.Player)
                {
                    curAttackIdx = 0;
                }
                // 보스 기믹 실패 시(두 플레이어 모두 사망 시) 해당 기믹 재실행
            }
        }

        round++;
    }

    private void UpdateRecovery()
    {
        recoverySlider.value += recoveryPerSuccess;
    }

    [PunRPC]
    void SetAttackType(AttackType newType)
    {
        attackType = newType;
    }
}
