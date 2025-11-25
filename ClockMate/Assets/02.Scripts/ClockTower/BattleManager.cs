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
/// 전투 흐름을 총괄하는 전투 매니저
/// 마스터가 전투 루틴 관리 및 동기화 정보 송신
/// </summary>
public class BattleManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, PhaseType> AttackNameToType;
    private Dictionary<PlayerAttackType, string> playerCutsceneNames;
    private Dictionary<PlayerAttackType, float> playerAttackTimeLimit;    // 플레이어 반격 제한시간

    private float _timer;   // 플레이어 제한 시간용 타이머

    [Header("공격 프리팹")]
    [SerializeField] private List<GameObject> bossAttackPrefabs;
    [SerializeField] private List<GameObject> playerAttackPrefabs;
    private GameObject _spawnedAttack;
    private AttackPattern curAttackPattern;
    private Coroutine _runCoroutine;

    private ScreenEffectController screenEffectController;
    private bool curAttackSuccess = false; // 현재 공격 성공 여부
    private bool isHandling = false; // 연출 실행 중
    private bool attackEnded = false; // 현재 공격 종료 여부
    public bool successBattle = false; // 전투 성공 여부
    public bool isInBattle { get; private set; } = false; // 전투 진행 중 여부

    // 보스 공격 오브젝트 풀
    [Header("오브젝트 풀")]
    public NetworkObjectPool<SwingPendulum> pendulumPool;
    public NetworkObjectPool<FallingClockHand> clockhandPool;

    [Header("전장 바닥")]
    public GameObject[] clockFace;  // 덮개
    public GameObject[] cogs;   // 톱니바퀴 복구 성공 후 활성화할 기본 배치된 톱니바퀴들

    public int round { get; private set; } = 1;
    public PhaseType phaseType { get; private set; } = PhaseType.SwingAttack;
    public PlayerAttackType playerAttackType { get; private set; } = PlayerAttackType.ClockHandRecovery;
    public FallingAttack currentFallingAttack { get; private set; }

    [Header("UI")]
    public Slider recoverySlider;
    public GameObject timeLimitUI;
    public TMP_Text timeLimitText;

    public float battleFieldRadius { get; private set; } = 11f; // 전장 반지름
    public readonly Vector3 BattleFieldCenter = Vector3.zero;
    private const float recoveryPerSuccess = 0.334f;
    private const float playerBossAttackHeight = 0f;

    private readonly PhaseType[] PhaseTypes = (PhaseType[])Enum.GetValues(typeof(PhaseType));
    private readonly PlayerAttackType[] PlayerAttackTypes = (PlayerAttackType[])Enum.GetValues(typeof(PlayerAttackType));

    public static BattleManager Instance { get; private set; }

    [Header("테스트용 변수")]
    public bool isCutSceneTriggerOn = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
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

        screenEffectController = FindObjectOfType<ScreenEffectController>();

        playerCutsceneNames = new Dictionary<PlayerAttackType, string>
        {
            { PlayerAttackType.ClockHandRecovery, "ClockHandRecovery_Cutscene" },
            { PlayerAttackType.CogwheelRecovery, "CogwheelRecovery_Cutscene" },
            { PlayerAttackType.ClockTowerOperation, "ClockTowerOperation_Cutscene" }
        };

        playerAttackTimeLimit = new Dictionary<PlayerAttackType, float>
        {
            { PlayerAttackType.ClockHandRecovery, 100f },
            { PlayerAttackType.CogwheelRecovery, 200f },
            { PlayerAttackType.ClockTowerOperation, 60f }
        };
    }

    // 테스트용 코드
    //public override void OnJoinedRoom()
    //{
    //    StartCoroutine(StartBattleCoroutine());
    //}

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    StartBattle();
    //}

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && phaseType == PhaseType.PlayerAttack)
            RunTimer();
    }

    public void StartBattle()
    {
        isInBattle = true;

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartBattleCoroutine());
    }

    /// <summary>
    /// 마스터가 전투 코루틴 시작
    /// </summary>
    private IEnumerator StartBattleCoroutine()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        yield return StartCoroutine(RunBattle());

        if(PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 전투 루틴 실행
    /// 공격 순서 관리 및 현재 공격 프리팹 스폰
    /// 공격 성공/실패 관리
    /// </summary>
    private IEnumerator RunBattle()
    {
        while (true)
        {
            // 마지막 반격에 성공하면 종료
            if (successBattle)
            {
                yield break;
            }

            // 플레이어 공격 페이즈일 때 시간 제한 설정 및 UI 동기화
            if (phaseType == PhaseType.PlayerAttack)
            {
                if (playerAttackType == PlayerAttackType.CogwheelRecovery)
                {
                    photonView.RPC(nameof(CogWheelTransition), RpcTarget.All);
                    yield return new WaitUntil(() => !isHandling);
                }

                _timer = playerAttackTimeLimit[playerAttackType];
                photonView.RPC(nameof(RPC_EnableTimeLimit), RpcTarget.All, true);
                photonView.RPC(nameof(RPC_SetQuestActive), RpcTarget.All, true);
            }

            BattleLifeManager.Instance.allowRevive = true;
            _spawnedAttack = SpawnCurrentAttack();

            attackEnded = false;
            // 공격 실행
            _runCoroutine = StartCoroutine(RunAttack(curAttackPattern));
            yield return new WaitUntil(() => attackEnded);
            // 공격 완료 후 대기 시간
            yield return new WaitForSeconds(1f);

            bool success = curAttackSuccess;
            curAttackSuccess = false;

            // 기믹 성공 여부에 따른 연출
            if (success)
            {
                photonView.RPC(nameof(HandleSuccess), RpcTarget.All);
            }
            else
            {
                photonView.RPC(nameof(HandleFailure), RpcTarget.All);
            }

            if (phaseType == PhaseType.PlayerAttack)
            {
                photonView.RPC(nameof(RPC_EnableTimeLimit), RpcTarget.All, false);
                photonView.RPC(nameof(RPC_SetQuestActive), RpcTarget.All, false);
            }

            yield return new WaitUntil(() => !isHandling);
        }
    }

    private GameObject SpawnCurrentAttack()
    {
        // 현재 수행될 공격 패턴 생성
        GameObject attackPrefab = GetCurrentPhasePrefab();
        GameObject spawnedAttackPrefab = PhotonNetwork.Instantiate("Prefabs/" + attackPrefab.name, Vector3.zero, Quaternion.identity);
        curAttackPattern = spawnedAttackPrefab.GetComponent<AttackPattern>();
        currentFallingAttack = phaseType == PhaseType.FallingAttack ? curAttackPattern as FallingAttack : null;

        return spawnedAttackPrefab;
    }

    private IEnumerator RunAttack(AttackPattern attackPattern)
    {
        yield return attackPattern.Run();
        attackEnded = true;
    }

    /// <summary>
    /// 보스/플레이어 공격 성공 연출
    /// </summary>
    [PunRPC]
    private IEnumerator HandleSuccess()
    {
        isHandling = true;

        if (phaseType == PhaseType.PlayerAttack)
        {
            // 연출 중 조작 금지
            GameManager.Instance.GetLocalCharacter().InputHandler.enabled = false;
            screenEffectController.IncreaseWarmth(); // 화면 따뜻함 효과 증가

            if(PhotonNetwork.IsMasterClient)
            {
                // 복구율 증가
                if (playerAttackType != PlayerAttackType.ClockTowerOperation)
                    photonView.RPC(nameof(RPC_UpdateRecovery), RpcTarget.All, recoveryPerSuccess);

                // 성공 컷씬 재생
                CutsceneSyncManager.Instance.PlayForAll(playerCutsceneNames[playerAttackType]);

                // 공격 관련 오브젝트 정리
                yield return StartCoroutine(RPC_CleanUpAttack());

                if(phaseType == PhaseType.PlayerAttack)
                {
                    photonView.RPC(nameof(RPC_PlacePlayerOnClockFace), RpcTarget.All);
                    photonView.RPC(nameof(RPC_ActivateCogs), RpcTarget.All);
                }

                TryAdvanceBossAttack();
                TryAdvancePlayerAttack();
                round++;
            }

            while (CutsceneSyncManager.Instance.IsBusy)
            {
                yield return null;
            }

            // 조작 금지 해제
            GameManager.Instance.GetLocalCharacter().InputHandler.enabled = true;
        }
        else
        {
            TryAdvanceBossAttack(); // 다음 보스 공격으로 넘어가기
        }

        isHandling = false;
    }

    /// <summary>
    /// 보스/플레이어 공격 실패 연출
    /// </summary>
    [PunRPC]
    private IEnumerator HandleFailure()
    {
        isHandling = true;

        if (phaseType == PhaseType.PlayerAttack)
        {
            GameManager.Instance.GetLocalCharacter().InputHandler.enabled = false;
            yield return StartCoroutine(screenEffectController.EnableGrayscale(true)); // 흑백 효과 및 페이드 아웃 시작
            yield return StartCoroutine(screenEffectController.FadeOut(3f));

            if(PhotonNetwork.IsMasterClient)
            {
                // 공격 관련 오브젝트 정리
                yield return StartCoroutine(RPC_CleanUpAttack());

                if (phaseType == PhaseType.PlayerAttack)
                {
                    photonView.RPC(nameof(RPC_PlacePlayerOnClockFace), RpcTarget.All);
                }
            }

            TryAdvanceBossAttack();
            round++;

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(screenEffectController.EnableGrayscale(false));
            yield return StartCoroutine(screenEffectController.FadeIn(3f));
            GameManager.Instance.GetLocalCharacter().InputHandler.enabled = true;
        }
        else
        {
            yield return StartCoroutine(FailBossAttackSequence());
        }

        isHandling = false;
    }

    /// <summary>
    /// 톱니바퀴 복구 연출
    /// 덮개가 사라진 후 플레이어는 잠시 멈추었다 떨어짐
    /// </summary>
    [PunRPC]
    private IEnumerator CogWheelTransition()
    {
        isHandling = true;

        CharacterBase localCharacter = GameManager.Instance.Characters[GameManager.Instance.SelectedCharacter];

        GameManager.Instance.SetLocalCharacterInput(false);
        localCharacter.GetComponent<Rigidbody>().useGravity = false;
        SetClockFaceActive(false);
        yield return new WaitForSeconds(0.5f);

        localCharacter.GetComponent<Rigidbody>().useGravity = true;
        GameManager.Instance.SetLocalCharacterInput(true);

        yield return new WaitUntil(() => localCharacter.IsGrounded);

        isHandling = false;
    }

    [PunRPC]
    private IEnumerator RPC_CleanUpAttack()
    {
        curAttackPattern?.CleanUpAttack();

        if (curAttackPattern != null)
        {
            yield return new WaitUntil(() => curAttackPattern.cleanUpEnded);
        }

        if (PhotonNetwork.IsMasterClient && _spawnedAttack != null)
        {
            PhotonNetwork.Destroy(_spawnedAttack);
            _spawnedAttack = null;
        }
    }

    /// <summary>
    /// 톱니바퀴 복구였다면 덮개 활성화 및 플레이어는 덮개 위로 이동
    /// </summary>
    [PunRPC]
    private void RPC_PlacePlayerOnClockFace()
    {
        if(!clockFace[0].gameObject.activeSelf)
        {
            CharacterBase character = GameManager.Instance.GetLocalCharacter();
            SetClockFaceActive(true);
            character.transform.position = new Vector3(character.transform.position.x, playerBossAttackHeight, character.transform.position.z);
        }
    }

    /// <summary>
    /// 전장 톱니바퀴 활성화
    /// </summary>
    [PunRPC]
    private void RPC_ActivateCogs()
    {
        if (!clockFace[0].gameObject.activeSelf)
        {
            foreach (var cog in cogs)
            {
                cog.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 복구율 갱신
    /// </summary>
    [PunRPC]
    public void RPC_UpdateRecovery(float value)
    {
        recoverySlider.value += value;
    }

    /// <summary>
    /// 복구율 반환
    /// </summary>
    public float GetCurrentRecovery()
    {
        return recoverySlider.value;
    }

    /// <summary>
    /// 현재 보스/플레이어 공격 프리팹 반환
    /// </summary>
    private GameObject GetCurrentPhasePrefab()
    {
        if (phaseType == PhaseType.PlayerAttack)
            return playerAttackPrefabs[(int)playerAttackType];
        else
            return bossAttackPrefabs[(int)phaseType];
    }

    /// <summary>
    /// 현재 공격 성공 여부 전달
    /// </summary>
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
    void TryAdvanceBossAttack()
    {
        if ((int)playerAttackType >= playerAttackPrefabs.Count) return;

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
    void TryAdvancePlayerAttack()
    {
        int index = (int)playerAttackType;

        if (index + 1 < PlayerAttackTypes.Length)
        {
            playerAttackType = PlayerAttackTypes[index + 1];

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_SetPlayerAttackType), RpcTarget.Others, (int)playerAttackType);
            }
        }
    }

    [PunRPC]
    private void RPC_SetPlayerAttackType(int newType)
    {
        playerAttackType = (PlayerAttackType)newType;
    }

    /// <summary>
    /// 보스 공격 실패 연출
    /// </summary>
    public IEnumerator FailBossAttackSequence()
    {
        GameManager.Instance.GetLocalCharacter().InputHandler.enabled = false;
        yield return StartCoroutine(screenEffectController.EnableGrayscale(true));
        yield return StartCoroutine(screenEffectController.FadeOut(3f));

        curAttackPattern?.CleanUpAttack();
        BattleLifeManager.Instance.ReviveAllPlayer();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(screenEffectController.EnableGrayscale(false));
        yield return StartCoroutine(screenEffectController.FadeIn(3f));
        GameManager.Instance.GetLocalCharacter().InputHandler.enabled = true;
    }
    
    /// <summary>
    /// 플레이어 반격 공격 시간 제한 UI 갱신
    /// </summary>
    [PunRPC]
    private void RPC_UpdateTimeLimitTxt(int time)
    {
        if (time < 0) time = 0;
        timeLimitText.text = time + "초";
    }

    /// <summary>
    /// 시간 제한 타이머 실행
    /// </summary>
    private void RunTimer()
    {
        _timer -= Time.deltaTime;
        photonView.RPC(nameof(RPC_UpdateTimeLimitTxt), RpcTarget.All, Mathf.CeilToInt(_timer));
    }

    /// <summary>
    /// 시간 제한이 끝났는지
    /// </summary>
    public bool IsTimeLimitEnd()
    {
        return _timer <= 0;
    }

    /// <summary>
    /// 시간 제한 UI 활성화
    /// </summary>
    [PunRPC]
    void RPC_EnableTimeLimit(bool isEnable)
    {
        timeLimitUI.SetActive(isEnable);
    }

    /// <summary>
    /// 덮개 활성화 설정
    /// </summary>
    private void SetClockFaceActive(bool isActive)
    {
        foreach (GameObject cf in clockFace)
        {
            cf.SetActive(isActive);
        }
    }

    /// <summary>
    /// 현재 공격을 중단하고 실패 처리
    /// </summary>
    public void StopAttackRun()
    {
        if(_runCoroutine != null)
        {
            StopCoroutine(_runCoroutine);
            _runCoroutine = null;
        }

        photonView.RPC("ReportAttackResult", RpcTarget.All, false);
        attackEnded = true;
    }

    [PunRPC]
    public void RPC_SetQuestActive(bool active)
    {
        PuzzleHUD puzzleHUD = GameObject.FindAnyObjectByType<PuzzleHUD>();
        if (puzzleHUD == null) return;
        
        if(active)
        {
            puzzleHUD.ShowAndUpdateQuest();
        }
        else
        {
            puzzleHUD.HideQuest();
        }
    }
}