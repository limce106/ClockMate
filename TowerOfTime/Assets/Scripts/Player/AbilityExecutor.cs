using System;
using System.Collections;
using System.Collections.Generic;
using DefineExtension;
using UnityEngine;
using static Define.Character;

/// <summary>
/// 캐릭터의 능력에 따라 상호작용 기능 실행
/// (ex. 아워 - 물건 들기, 밀리 - 벽타기)
/// </summary>
public class AbilityExecutor : MonoBehaviour
{
    [SerializeField] private PlayerBase _player;

    private void Awake()
    {
        if (_player != null)
        {
            _player.OnInteract += TryAbility;
        }
    }

    public void TryAbility()
    {
        foreach (Ability ability in _player.CharacterId.GetAbilities())
        {
            switch (ability)
            {
                case Ability.LiftObject:
                    TryLift();
                    break;
                case Ability.WallClimb:
                    TryClimb();
                    break;
            }
        }
    }

    private void TryLift()
    {
        Debug.Log("물건 들기");
    }

    private void TryClimb()
    {
        Debug.Log("벽 타기");
    }
}
