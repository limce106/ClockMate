using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIHpBar : UIBase {
    [SerializeField] private Image imgHpTop;
    [SerializeField] private Image imgHpMiddle;

    [SerializeField] private float damageLag = 0.15f;  // 지연 시작 대기
    [SerializeField] private float delayBarSpeed = 1.2f; // 지연바 감소 속도
    
    private Coroutine _delayRoutine;
    private float _targetFill; // 지연 목표치;

    /// <summary>
    /// SledHP에서 호출하여 hp바 fillAmount를 업데이트한다.
    /// </summary>
    public void UpdateHpBar(float maxHP, float currentHP)
    {
        float target = Mathf.Clamp01(maxHP <= 0 ? 0f : (float)currentHP / maxHP);

        // 빨간바 즉시 반영
        imgHpTop.fillAmount = target; // 즉시

        // 지연바
        if (target < imgHpMiddle.fillAmount)
        {
            // 이미 지연바 감소 중이라면 목표치만 갱신
            _targetFill = target;
            _delayRoutine ??= StartCoroutine(FollowWithDelay());
        }
        else
        {
            // 체력이 늘어나는 경우 즉시 반영
            if (_delayRoutine != null)
            {
                StopCoroutine(_delayRoutine);
                _delayRoutine = null;
            }

            imgHpMiddle.fillAmount = target;
        }
    }

    private IEnumerator FollowWithDelay()
    {
        // 감소 시작 전 일정 시간 대기
        yield return new WaitForSeconds(damageLag);

        // 지연바가 목표에 도달할 때까지 반복
        while (true)
        {
            // 목표가 갱신될 수 있으므로 루프마다 현재 목표 사용
            float next = Mathf.MoveTowards(
                imgHpMiddle.fillAmount, _targetFill, delayBarSpeed * Time.deltaTime);
            imgHpMiddle.fillAmount = next;

            // 목표에 도달하면 종료
            if (Mathf.Approximately(next, _targetFill))
            {
                // 더 작은 목표로 갱신되지 않았으면 끝
                if (Mathf.Approximately(_targetFill, next))
                    break;
            }

            yield return null;
        }

        _delayRoutine = null;
    }
}
