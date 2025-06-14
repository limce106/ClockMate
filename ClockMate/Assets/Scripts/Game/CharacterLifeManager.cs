using System.Collections;
using UnityEngine;

public class CharacterLifeManager : MonoSingleton<CharacterLifeManager>
{
    private CharacterBase _deadCharacter;
    private Vector3 _revivePosition;
    private UIRevive _uiRevive;
    private UIGameOver _uiGameOver;

    private void Update()
    {
        if (_uiGameOver is not null && _uiGameOver.gameObject.activeSelf) return;
        // 캐릭터 y좌표 일정 이하가 되면 죽음 처리
        foreach (var character in GameManager.Instance.Characters.Values)
        {
            if (character.transform.position.y < -10f && character != _deadCharacter)
            {
                CharacterDeath(character);
            }
        }

        if (_deadCharacter is not null)
        {
            // 생존한 플레이어가 살리기 상호작용하면 죽은 플레이어 살아나게
            if (Input.GetKeyDown(KeyCode.E))
            {
                Revive();
            }
        }
    }

    protected override void Init()
    {
        _revivePosition = new Vector3(-4.94700003f,0.97353971f,6.51000023f);
    }

    private void Revive()
    {
        // 죽은 플레이어 부활 지점으로 이동, IdleState로 변경
        _deadCharacter.transform.position = _revivePosition;
        _deadCharacter.ChangeState<IdleState>();
        _deadCharacter.gameObject.SetActive(true);

        UIManager.Instance.Close(_uiRevive); // 살려준 플레이어의 살리기 UI는 숨김 처리
        
        StartCoroutine(InvincibilityCoroutine(2.0f));
        _deadCharacter = null;
    }

    public void CharacterDeath(CharacterBase deadCharacter)
    {
        GameManager.Instance.CurrentStage.ReduceReviveCount();
        Debug.Log($"남은 목숨: {GameManager.Instance.CurrentStage.LeftReviveCount}");

        if (_deadCharacter is not null || GameManager.Instance.CurrentStage.IsReviveImpossible)
        {
            // 부활이 불가능한 상태라면 게임오버
            GameManager.Instance.SetAllCharactersActive(false);
            _uiGameOver = UIManager.Instance.Show<UIGameOver>("UIGameOver");
            return;
        } 
        _deadCharacter = deadCharacter;
        _deadCharacter.gameObject.SetActive(false);
        // 사망한 위치와 가장 가까운 부활 가능 지점에 영혼(?) 표시
        _uiRevive = UIManager.Instance.Show<UIRevive>("UIRevive");// 살아있는 캐릭터에게는 살리기 UI 표시
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        // 무적 처리
        yield return new WaitForSeconds(duration);
        // 무적 해제
    }
}