using UnityEngine;
using UnityEngine.SceneManagement;
using static Define.Battle;

public class DeadState : IState
{
    
    private readonly CharacterBase _character;

    public DeadState(CharacterBase character) =>_character = character;
    
    public void Enter()
    {
        if(SceneManager.GetActiveScene().ToString() != "ClockTower")
        {
            _character.gameObject.SetActive(false);
            StageLifeManager.Instance.HandleDeath(_character);
        }
        else
        {
            if(BattleManager.Instance.phaseType == PhaseType.SwingAttack)
            {
                // SwingAttack으로 인해 사망했다면 마지막 위치 저장
                Vector3 hitPos = _character.transform.position;
                BattleLifeManager.Instance.RecordHitPosition(_character, hitPos);
            }

            BattleLifeManager.Instance.HandleDeath(_character);
        }
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        if (SceneManager.GetActiveScene().ToString() != "ClockTower")
        {
            _character.gameObject.SetActive(true);
        }
    }
}
