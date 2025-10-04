using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

public class BattleLifeManager : MonoBehaviourPun
{
    private HashSet<int> _deadPlayers = new HashSet<int>();
    private Dictionary<CharacterBase, Vector3> _lastHitPositions = new Dictionary<CharacterBase, Vector3>();
    private Coroutine _reviveCoroutine;

    public static BattleLifeManager Instance { get; private set; }

    private const float ReviveDelay = 3f;
    private const float safeTime = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void HandleDeath(CharacterBase character, DeathType deathType)
    {
        int id = character.GetComponent<PhotonView>().ViewID;
        photonView.RPC(nameof(RPC_AddDeadPlayers), RpcTarget.All, id);

        if (_deadPlayers.Count == 1)
        {
            _reviveCoroutine = StartCoroutine(Revive(character, deathType));
        }
        else if (_deadPlayers.Count == 2)
        {
            photonView.RPC(nameof(RPC_StopReviveCoroutine), RpcTarget.All);
            photonView.RPC(nameof(RPC_ClearDeadPlayers), RpcTarget.All);

            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
            BattleManager.Instance.StopCurAttackPattern();
        }
    }

    private IEnumerator Revive(CharacterBase character, DeathType deathType)
    {
        yield return new WaitForSeconds(ReviveDelay);

        IReviveStrategy strategy = GetStrategy(character, deathType);
        Vector3 revivePos = strategy.GetRevivePosition();

        character.ChangeState<IdleState>();
        character.transform.position = revivePos;

        int id = character.GetComponent<PhotonView>().ViewID;
        photonView.RPC(nameof(RPC_RemoveDeadPlayers), RpcTarget.All, id);

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
    /// 전투 오브젝트와 충돌 시 마지막 위치 저장용
    /// </summary>
    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        _lastHitPositions[character] = pos;
    }

    /// <summary>
    /// 사망 원인 기준으로 부활 전략 선택
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

    [PunRPC]
    private void RPC_AddDeadPlayers(int viewId)
    {
        _deadPlayers.Add(viewId);
    }

    [PunRPC]
    private void RPC_RemoveDeadPlayers(int viewId)
    {
        _deadPlayers.Remove(viewId);
    }

    [PunRPC]
    private void RPC_ClearDeadPlayers()
    {
        _deadPlayers.Clear();
    }

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
