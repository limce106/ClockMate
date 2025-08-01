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
