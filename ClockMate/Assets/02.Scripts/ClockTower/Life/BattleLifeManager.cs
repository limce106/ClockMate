using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;
using static Define.Battle;

public class BattleLifeManager : MonoBehaviour
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
        int id = character.GetComponent<PhotonView>().ViewID;
        deadPlayers.Add(id);

        if (deadPlayers.Count == 1)
        {
            StartCoroutine(Revive(character));
        }
        else
        {
            // TODO ���� ����
        }
    }

    private IEnumerator Revive(CharacterBase character)
    {
        yield return new WaitForSeconds(3f);

        IReviveStrategy strategy = GetStrategy(character);

        Vector3 revivePos = strategy.GetRevivePosition();
        character.transform.position = revivePos;

        character.ChangeState<IdleState>();
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
                return new SwingReviveStrategy(lastHitPositions[character]);
            case AttackType.SmashAttack:
                return new SmashReviveStrategy(BattleManager.Instance.currentSmashAttack);
            case AttackType.PlayerAttack:
            default:
                return new DefaultReviveStrategy(Vector3.zero);
        }
    }
}
