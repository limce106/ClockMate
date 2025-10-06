using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

/// <summary>
/// 전투 중 플레이어 생명 및 부활 관리를 담당하는 매니저
/// </summary>
public class BattleLifeManager : MonoBehaviourPun
{
    private HashSet<int> _deadPlayers = new HashSet<int>(); // 죽은 플레이어의 포톤 뷰 ID 저장
    private Dictionary<CharacterBase, Vector3> _lastHitPositions = new Dictionary<CharacterBase, Vector3>(); // 죽기 전 충돌 위치

    private Coroutine _reviveCoroutine; // 로컬 부활 코루틴
    public bool allowRevive = true;

    public static BattleLifeManager Instance { get; private set; }

    private const float ReviveDelay = 3f; // 부활 딜레이
    private const float safeTime = 0.1f; // 부활 후 물리 영향을 받지 않는 시간

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 플레이어 사망 전달
    /// </summary>
    [PunRPC]
    public void RPC_ReportDeath(int viewID)
    {
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonView targetView = PhotonView.Find(viewID);
        if(targetView == null) return;

        CharacterBase character = targetView.GetComponent<CharacterBase>();
        if (character == null) return;

        _deadPlayers.Add(viewID);
        Debug.Log("_deadPlayerNum: " + _deadPlayers.Count);
        HandleDeath(character);
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    public void HandleDeath(CharacterBase character)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (_deadPlayers.Count == 1)
        {
            _reviveCoroutine = StartCoroutine(ReviveAfterDelay(character));
        }
        else if (_deadPlayers.Count == 2)
        {
            allowRevive = false;
            photonView.RPC(nameof(RPC_StopReviveCoroutine), RpcTarget.All);
            BattleManager.Instance.StopAttackRun();
        }
    }

    /// <summary>
    /// 지연 후 부활
    /// </summary>
    private IEnumerator ReviveAfterDelay(CharacterBase character)
    {
        yield return new WaitForSeconds(ReviveDelay);

        if(!allowRevive)
            yield break;

        StartCoroutine(Revive(character));
    }

    /// <summary>
    /// 플레이어 부활 수행
    /// </summary>
    private IEnumerator Revive(CharacterBase character)
    {
        _lastHitPositions.TryGetValue(character, out Vector3 pos);
        Vector3 revivePos = pos;

        int viewID = character.photonView.ViewID;
        _deadPlayers.Remove(viewID);

        RPCManager.Instance.photonView.RPC("RPC_SetObjectActive", RpcTarget.All, character.photonView.ViewID, true);
        character.ChangeState<IdleState>();
        character.transform.position = revivePos;

        if (BattleManager.Instance.phaseType == PhaseType.SwingAttack)
        {
            // SwingAttack 중 부활 시 플레이어 미끄러짐 방지
            Rigidbody rb = character.GetComponent<Rigidbody>();

            rb.isKinematic = true;
            yield return new WaitForSeconds(safeTime);
            rb.isKinematic = false;
        }

        _reviveCoroutine = null;
    }

    /// <summary>
    /// 로컬 플레이어 부활
    /// </summary>
    [PunRPC]
    public void RPC_ReviveLocalPlayer()
    {
        CharacterBase character = GameManager.Instance.GetLocalCharacter();
        StartCoroutine(Revive(character));
    }

    /// <summary>
    /// 모든 클라이언트의 로컬 플레이어 부활
    /// </summary>
    public void ReviveAllPlayer()
    {
        photonView.RPC(nameof(RPC_ReviveLocalPlayer), RpcTarget.All);
    }

    /// <summary>
    /// 전투 오브젝트와 충돌한 위치(부활 위치) 저장
    /// </summary>
    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        _lastHitPositions[character] = pos;
    }

    /// <summary>
    /// 사망 원인에 따라 부활 전략 선택 (현재 사용 안 함)
    /// </summary>
    private IReviveStrategy GetStrategy(CharacterBase character, DeathType deathType)
    {
        switch(deathType)
        {
            case DeathType.Collision:
                if (_lastHitPositions.TryGetValue(character, out Vector3 pos))
                {
                    return new BattleHitReviveStrategy(pos);
                }
                else
                {
                    Debug.Log("Can't Get Last Hit Position!");
                    return new DefaultReviveStrategy(BattleManager.Instance.BattleFieldCenter);
                }
            case DeathType.Fall:
                return new DefaultReviveStrategy(BattleManager.Instance.BattleFieldCenter);
            default:
                return new DefaultReviveStrategy(BattleManager.Instance.BattleFieldCenter);
        }
    }

    /// <summary>
    /// 진행 중인 부활 코루틴 중단
    /// </summary>
    [PunRPC]
    private void RPC_StopReviveCoroutine()
    {
        if (_reviveCoroutine != null)
        {
            StopCoroutine(_reviveCoroutine);
            _reviveCoroutine = null;
        }
    }
}
