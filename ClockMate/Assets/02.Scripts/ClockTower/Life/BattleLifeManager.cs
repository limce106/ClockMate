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

    public readonly Vector3 BattleFieldCenter = Vector3.zero;

    public static BattleLifeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void HandleDeath(CharacterBase character)
    {
        int id = character.GetComponent<PhotonView>().ViewID;
        deadPlayers.Add(id);

        if (deadPlayers.Count == 1)
        {
            StartCoroutine(Revive(character));
        }
        else
        {
            BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
            BattleManager.Instance.StopCurAttackPattern();
            deadPlayers.Clear();
        }
    }

    private IEnumerator Revive(CharacterBase character)
    {
        yield return new WaitForSeconds(3f);

        IReviveStrategy strategy = GetStrategy(character);

        Vector3 revivePos = strategy.GetRevivePosition();
        character.transform.position = revivePos;

        character.ChangeState<IdleState>(); // TODO 정상적으로 동기화 되는지 확인할 것!
        deadPlayers.Remove(character.GetComponent<PhotonView>().ViewID);
    }

    /// <summary>
    /// SwingPendulum과 부딪혔을 때 마지막 위치 저장용
    /// </summary>
    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        lastHitPositions[character] = pos;
    }

    /// <summary>
    /// 부활 시점 기준으로 부활 전략 선택
    /// </summary>
    private IReviveStrategy GetStrategy(CharacterBase character)
    {
        switch(BattleManager.Instance.phaseType)
        {
            case PhaseType.SwingAttack:
                if (lastHitPositions.TryGetValue(character, out Vector3 pos))
                {
                    return new SwingReviveStrategy(pos);
                }
                else
                {
                    // 낙사 또는 미기록일때
                    return new DefaultReviveStrategy(BattleFieldCenter);
                }
            case PhaseType.SmashAttack:
                return new SmashReviveStrategy(BattleManager.Instance.currentSmashAttack);
            case PhaseType.PlayerAttack:
            default:
                return new DefaultReviveStrategy(BattleFieldCenter);
        }
    }
}
