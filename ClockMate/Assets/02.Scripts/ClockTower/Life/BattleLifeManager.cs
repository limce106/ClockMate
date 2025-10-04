using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Battle;

public class BattleLifeManager : MonoBehaviourPun
{
    private int _deadPlayerNum = 0;
    private Dictionary<CharacterBase, Vector3> _lastHitPositions = new Dictionary<CharacterBase, Vector3>();
    public DeathType localDeathType = DeathType.None;
    private Coroutine _reviveCoroutine;
    public bool isAllPlayerDead = false;

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
        photonView.RPC(nameof(RPC_SetDeadPlayerNum), RpcTarget.All, _deadPlayerNum + 1);
        localDeathType = deathType;

        if (_deadPlayerNum == 1)
        {
            _reviveCoroutine = StartCoroutine(ReviveAfterDelay(character, deathType));
        }
        else if (_deadPlayerNum == 2)
        {
            photonView.RPC(nameof(RPC_StopReviveCoroutine), RpcTarget.All);
            photonView.RPC(nameof(RPC_SetDeadPlayerNum), RpcTarget.All, 0);

            isAllPlayerDead = true;
        }
    }

    private IEnumerator ReviveAfterDelay(CharacterBase character, DeathType deathType)
    {
        yield return new WaitForSeconds(ReviveDelay);

        StartCoroutine(Revive(character, deathType));
    }

    private IEnumerator Revive(CharacterBase character, DeathType deathType)
    {
        IReviveStrategy strategy = GetStrategy(character, deathType);
        Vector3 revivePos = strategy.GetRevivePosition();

        character.ChangeState<IdleState>();
        character.transform.position = revivePos;

        int id = character.GetComponent<PhotonView>().ViewID;
        photonView.RPC(nameof(RPC_SetDeadPlayerNum), RpcTarget.All, _deadPlayerNum - 1);
        localDeathType = DeathType.None;

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

    [PunRPC]
    public void RPC_ReviveLocalPlayer()
    {
        CharacterBase character = GameManager.Instance.GetLocalCharacter();
        StartCoroutine(Revive(character, localDeathType));
    }

    public void ReviveAllPlayer()
    {
        if (isAllPlayerDead)
        {
            photonView.RPC(nameof(RPC_ReviveLocalPlayer), RpcTarget.All);
        }
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
    private void RPC_SetDeadPlayerNum(int num)
    {
        _deadPlayerNum = num;
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
