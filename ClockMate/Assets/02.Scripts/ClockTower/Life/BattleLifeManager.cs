using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;
using static Define.Battle;

public class BattleLifeManager : MonoBehaviourPun
{
    private HashSet<int> deadPlayers = new HashSet<int>();
    private Dictionary<CharacterBase, Vector3> lastHitPositions = new Dictionary<CharacterBase, Vector3>();

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
        deadPlayers.Add(id);

        if (deadPlayers.Count == 1)
        {
            StartCoroutine(Revive(character, deathType));
        }
        else if (deadPlayers.Count == 2)
        {
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
            BattleManager.Instance.StopCurAttackPattern();
            deadPlayers.Clear();
        }
    }

    private IEnumerator Revive(CharacterBase character, DeathType deathType)
    {
        yield return new WaitForSeconds(ReviveDelay);

        IReviveStrategy strategy = GetStrategy(character, deathType);
        Vector3 revivePos = strategy.GetRevivePosition();

        character.ChangeState<IdleState>();
        character.transform.position = revivePos;

        if(BattleManager.Instance.phaseType == PhaseType.SwingAttack)
        {
            // SwingAttack 중 부활 시 플레이어 미끄러짐 방지
            Rigidbody rb = character.GetComponent<Rigidbody>();

            rb.isKinematic = true;
            yield return new WaitForSeconds(safeTime);
            rb.isKinematic = false;
        }

        deadPlayers.Remove(character.GetComponent<PhotonView>().ViewID);
    }

    /// <summary>
    /// 전투 오브젝트와 충돌 시 마지막 위치 저장용
    /// </summary>
    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        lastHitPositions[character] = pos;
    }

    /// <summary>
    /// 사망 원인 기준으로 부활 전략 선택
    /// </summary>
    private IReviveStrategy GetStrategy(CharacterBase character, DeathType deathType)
    {
        switch(deathType)
        {
            case DeathType.Collision:
                if (lastHitPositions.TryGetValue(character, out Vector3 pos))
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
}
