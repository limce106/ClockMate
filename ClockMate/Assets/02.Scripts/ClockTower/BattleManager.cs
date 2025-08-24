using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define.Battle;

/// <summary>
/// 전투 흐름 제어
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, PhaseType> AttackNameToType;

    private float _timer;   // 플레이어 제한 시간용 타이머
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
    public NetworkObjectPool<FallingClockHand> clockhandPool;

    public PhaseType phaseType { get; private set; } = PhaseType.PlayerAttack;
    public PlayerAttackType playerAttackType { get; private set; } = PlayerAttackType.ClockTowerOperation;
    public FallingAttack currentFallingAttack { get; private set; }

    [Header("UI")]
    public Slider recoverySlider;

    [Tooltip("인스펙터에서 값 변경하지 말 것")]
    public int round = 1;

    private const float playerAttackTimeLimit = 10f;
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
        //if (StageLifeManager.Instance != null)
        //    Destroy(StageLifeManager.Instance);

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
    //    if (PhotonNetwork.IsMasterClient)
    //        StartCoroutine(StartBattle());
    //}

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && phaseType == PhaseType.PlayerAttack)
            RunTimer();
    }

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

            // 플레이어 공격 페이즈일 때 시간 제한 설정 및 UI 동기화
            if (phaseType == PhaseType.PlayerAttack)
            {
                _timer = playerAttackTimeLimit;
                photonView.RPC(nameof(RPC_EnableTimeLimit), RpcTarget.All, true);
            }
            else
            {
                photonView.RPC(nameof(RPC_EnableTimeLimit), RpcTarget.All, false);
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

            if (success)
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
                UpdateRecovery(recoveryPerSuccess);

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

    public void UpdateRecovery(float value)
    {
        recoverySlider.value += value;
    }

    public float GetCurrentRecovery()
    {
        return recoverySlider.value;
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
    
    [PunRPC]
    private void RPC_UpdateTimeLimitTxt(int time)
    {
        if (time < 0) time = 0;
        timeLimitText.text = time + "초";
    }

    private void RunTimer()
    {
        _timer -= Time.deltaTime;
        photonView.RPC(nameof(RPC_UpdateTimeLimitTxt), RpcTarget.All, Mathf.CeilToInt(_timer));
    }

    /// <summary>
    /// 시간 제한이 끝났는지
    /// </summary>
    /// <returns></returns>
    public bool IsTimeLimitEnd()
    {
        return _timer <= 0;
    }

    [PunRPC]
    void RPC_EnableTimeLimit(bool isEnable)
    {
        timeLimitText.GetComponent<TMP_Text>().enabled = isEnable;
    }
}
