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

        character.ChangeState<IdleState>(); // TODO ���������� ����ȭ �Ǵ��� Ȯ���� ��!
        deadPlayers.Remove(character.GetComponent<PhotonView>().ViewID);
    }

    /// <summary>
    /// SwingPendulum�� �ε����� �� ������ ��ġ �����
    /// </summary>
    public void RecordHitPosition(CharacterBase character, Vector3 pos)
    {
        lastHitPositions[character] = pos;
    }

    /// <summary>
    /// ��Ȱ ���� �������� ��Ȱ ���� ����
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
                    // ���� �Ǵ� �̱���϶�
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
