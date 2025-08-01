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
        character.ChangeState<DeadState>();

        int id = character.GetComponent<PhotonView>().ViewID;
        deadPlayers.Add(id);

        if (deadPlayers.Count == 1)
        {
            StartCoroutine(Revive(character));
        }
        else
        {
            ScreenEffectController screenEffectController = FindAnyObjectByType<ScreenEffectController>();
            StartCoroutine(screenEffectController.FailBossAttackSequence());
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

    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        lastHitPositions[character] = pos;
    }

    private IReviveStrategy GetStrategy(CharacterBase character)
    {
        switch(BattleManager.Instance.attackType)
        {
            case AttackType.SwingAttack:
                if (lastHitPositions.TryGetValue(character, out Vector3 pos))
                {
                    return new SwingReviveStrategy(pos);
                }
                else
                {
                    Debug.LogWarning($"{character.name} 부활 위치 저장 안 됨");
                    return new DefaultReviveStrategy(Vector3.zero);
                }
            case AttackType.SmashAttack:
                return new SmashReviveStrategy(BattleManager.Instance.currentSmashAttack);
            case AttackType.PlayerAttack:
            default:
                return new DefaultReviveStrategy(Vector3.zero);
        }
    }
}
