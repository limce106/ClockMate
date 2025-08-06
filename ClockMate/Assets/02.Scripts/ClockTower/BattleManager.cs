using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define.Battle;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 전투 흐름 제어
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, PhaseType> AttackNameToType;

    public TMP_Text timeLimitText;
    [SerializeField] private List<GameObject> bossAttackPrefabs;
    [SerializeField] private List<GameObject> playerAttackPrefabs;
    private AttackPattern curAttackPattern;
    private ScreenEffectController screenEffectController;
    private bool curAttackSuccess = false;

    // 전장 범위
    public Vector2 minBattleFieldXZ;
    public Vector2 maxBattleFieldXZ;

    // 보스 공격 오브젝트 풀
    public NetworkObjectPool<SwingPendulum> pendulumPool;
    public NetworkObjectPool<FallingNeedle> needlePool;

    public PhaseType phaseType { get; private set; } = PhaseType.PlayerAttack;
    public PlayerAttackType playerAttackType { get; private set; } = PlayerAttackType.ClockNeedleRecovery;
    public FallingAttack currentFallingAttack { get; private set; }

    [Header("UI")]
    public Slider recoverySlider;

    [Tooltip("인스펙터에서 값 변경하지 말 것")]
    public int round = 1;

    public readonly Vector3 BattleFieldCenter = new Vector3(0f, 1f, 0f);
    private const float recoveryPerSuccess = 0.334f;
    private readonly PhaseType[] PhaseTypes = (PhaseType[])Enum.GetValues(typeof(PhaseType));
    private readonly PlayerAttackType[] PlayerAttackTypes = (PlayerAttackType[])Enum.GetValues(typeof(PlayerAttackType));

    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // AttackNameToType 초기화
        AttackNameToType = new Dictionary<string, PhaseType>
        {
            {"SwingAttack", PhaseType.SwingAttack },
            {"DropAttack", PhaseType.FallingAttack },
            {"PlayerAttack", PhaseType.PlayerAttack },
        };

        // StageLifeManager 파괴
        if (StageLifeManager.Instance != null)
            Destroy(StageLifeManager.Instance);

        screenEffectController = FindObjectOfType<ScreenEffectController>();
    }

    void Start()
    {
        StartCoroutine(StartBattle());
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(StartBattle());
    }

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    if(PhotonNetwork.IsMasterClient)
    //        StartCoroutine(StartBattle());
    //}

    private IEnumerator StartBattle()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        yield return StartCoroutine(RunBattle());

        // TODO 성공 연출
    }

    private IEnumerator RunBattle()
    {
        while(true)
        {
            // 마지막 반격에 성공하면 종료
            if(phaseType == PhaseType.PlayerAttack && (int)playerAttackType >= playerAttackPrefabs.Count)
            {
                yield break;
            }

            GameObject attackPrefab = GetCurrentPhasePrefab();
            GameObject spawnedAttack = PhotonNetwork.Instantiate("Prefabs/" + attackPrefab.name, Vector3.zero, Quaternion.identity);
            curAttackPattern = spawnedAttack.GetComponent<AttackPattern>();
            currentFallingAttack = phaseType == PhaseType.FallingAttack ? curAttackPattern as FallingAttack : null;

            yield return StartCoroutine(curAttackPattern.Run());
            // 공격 완료 후 대기 시간
            yield return new WaitForSeconds(1f);
            PhotonNetwork.Destroy(spawnedAttack);

            bool success = curAttackSuccess;
            curAttackSuccess = false;

            if(success)
            {
                yield return StartCoroutine(HandleSuccess());
            }
            else
            {
                yield return StartCoroutine(HandleFailure());
            }
        }
    }

    private IEnumerator HandleSuccess()
    {
        if(phaseType == PhaseType.PlayerAttack)
        {
            if(playerAttackType != PlayerAttackType.ClockTowerOperation)
                UpdateRecovery();

            screenEffectController.IncreaseWarmth();

            photonView.RPC(nameof(TryAdvancePlayerAttack), RpcTarget.All);
            round++;

            // 마지막 반격 성공 시 종료
            if ((int)playerAttackType >= playerAttackPrefabs.Count)
            {
                yield break;
            }
        }

        photonView.RPC(nameof(TryAdvanceBossAttack), RpcTarget.All);
    }

    private IEnumerator HandleFailure()
    {
        if (phaseType == PhaseType.PlayerAttack)
        {
            photonView.RPC(nameof(TryAdvanceBossAttack), RpcTarget.All);
            round++;
        }
        else
        {
            yield return StartCoroutine(FailBossAttackSequence());
        }
    }

    private void UpdateRecovery()
    {
        recoverySlider.value += recoveryPerSuccess;
    }

    public void StopCurAttackPattern()
    {
        curAttackPattern?.CancelAttack();
    }

    private GameObject GetCurrentPhasePrefab()
    {
        if (phaseType == PhaseType.PlayerAttack)
            return playerAttackPrefabs[(int)playerAttackType];
        else
            return bossAttackPrefabs[(int)phaseType];
    }

    [PunRPC]
    public void ReportAttackResult(bool success, PhotonMessageInfo info)
    {

        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        curAttackSuccess = success;
    }

    /// <summary>
    /// 다음 페이즈로 이동
    /// </summary>
    [PunRPC]
    void TryAdvanceBossAttack()
    {
        int index = (int)phaseType;

        if (index + 1 < PhaseTypes.Length)
        {
            phaseType = PhaseTypes[index + 1];
        }
        else
        {
            phaseType = 0;
        }
    }

    /// <summary>
    /// 다음 플레이어 공격으로 이동
    /// </summary>
    [PunRPC]
    void TryAdvancePlayerAttack()
    {
        int index = (int)playerAttackType;

        if (index +1 < PlayerAttackTypes.Length)
        {
            playerAttackType = PlayerAttackTypes[index + 1];
        }
    }

    public IEnumerator FailBossAttackSequence()
    {
        yield return StartCoroutine(screenEffectController.EnableGrayscale(true));
        yield return StartCoroutine(screenEffectController.FadeOut(3f));

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(screenEffectController.EnableGrayscale(false));
        yield return StartCoroutine(screenEffectController.FadeIn(3f));
    }
}
