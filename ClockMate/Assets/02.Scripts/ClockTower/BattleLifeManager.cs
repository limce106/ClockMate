using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define.Character;

public class BattleLifeManager : MonoBehaviour
{
    private HashSet<int> deadPlayers = new HashSet<int>();

    public void HandleDeath(CharacterBase deadCharacter, Vector3 revivePos)
    {
        deadCharacter.ChangeState<DeadState>();

        int id = deadCharacter.GetComponent<PhotonView>().ViewID;
        deadPlayers.Add(id);

        if (deadPlayers.Count == 1)
        {
            StartCoroutine(Revive(deadCharacter, revivePos));
        }
        else
        {

        }
    }

    private IEnumerator Revive(CharacterBase deadCharacter, Vector3 revivePos)
    {
        yield return new WaitForSeconds(3f);

        deadCharacter.transform.position = revivePos;
        deadCharacter.ChangeState<IdleState>();

        deadPlayers.Remove(deadCharacter.GetComponent<PhotonView>().ViewID);
    }
}
