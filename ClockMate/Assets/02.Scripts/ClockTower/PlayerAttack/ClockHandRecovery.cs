using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockHandRecovery : AttackPattern
{
    [SerializeField] private Image hourClockHandUI;
    [SerializeField] private Image minuteClockHandUI;

    // ����� �� ��ǥ �ð�
    private int targetHour;
    private int targetMinute;

    private GameObject hourClockHand;
    private GameObject minuteClockHand;

    private bool isEnd = false;

    private const float timeLimit = 30f;

    private const string HourClockHandPrefabPath = "Prefabs/RecoveryHourClockHand";
    private const string MinuteClockHandPrefabPath = "Prefabs/RecoveryMinuteClockHand";
    private const float MinDistance = 5f;   // ��ħ, ��ħ ���� ��ġ ���� �ּ� �Ÿ�
    private const float AnswerMargin = 10f; // ������ �ð� �ٴõ��� ����� ��ġ�� �ʵ��� ���� �α�
    private const float AnswerOffset = 3f; // ���� ���� ��� ����
    private const float SpawnPosY = 0.65f;   // ���� ��ġ Y��

    protected override void Init()
    {
        
    }

    private void Start()
    {
        ShowRandomTargetTime();
        SpawnClockHands();
    }

    private void ShowRandomTargetTime()
    {
        targetHour = Random.Range(1, 13);
        targetMinute = Random.Range(1, 12) * 5;     // 5�� ������ ������

        hourClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetHourAngle());
        minuteClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetMinuteAngle());

        hourClockHandUI.GetComponent<Image>().enabled = true;
        minuteClockHandUI.GetComponent<Image>().enabled = true;
        BattleManager.Instance.timeLimitText.GetComponent<TMP_Text>().enabled = true;
    }

    float GetTargetHourAngle()
    {
        return (targetHour % 12) * 30f;
    }

    float GetTargetMinuteAngle()
    {
        return targetMinute * 6f;
    }

    void SpawnClockHands()
    {
        float targetHourAngle = GetTargetHourAngle();
        float targetMinuteAngle = GetTargetMinuteAngle();

        float randomHourAngle = GetRandomAngleExcluding(targetHourAngle);
        float randomMinuteAngle = GetRandomAngleExcluding(targetHourAngle, randomHourAngle);

        Vector3 SpawnPos = BattleManager.Instance.BattleFieldCenter;
        SpawnPos.y = SpawnPosY;

        hourClockHand = PhotonNetwork.Instantiate(HourClockHandPrefabPath, SpawnPos, Quaternion.Euler(0, -randomHourAngle, 0));
        minuteClockHand = PhotonNetwork.Instantiate(MinuteClockHandPrefabPath, SpawnPos, Quaternion.Euler(0, -randomMinuteAngle, 0));
    }

    float GetRandomAngleExcluding(float avoidAngle, float? hourAngle = null)
    {
        while(true)
        {
            float randomAngle = Random.Range(0f, 360f);

            float diffToAvoid = AngleDiff(randomAngle, avoidAngle);
            float diffToOther = hourAngle.HasValue ? AngleDiff(randomAngle, hourAngle.Value) : float.MaxValue;

            if (diffToAvoid > AnswerMargin && diffToOther > MinDistance)
                return randomAngle;
        }
    }

    float AngleDiff(float a, float b)
    {
        float diff = Mathf.Abs(a - b) % 360f;
        return diff > 180f ? 360f - diff : diff;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0)
            angle += 360f;

        return angle;
    }

    bool IsCorrectTime()
    {
        if(hourClockHand == null || minuteClockHand == null)
            return false;

        float hourClockHandAngle = NormalizeAngle(hourClockHand.transform.localEulerAngles.y);
        float minuteClockHandAngle = NormalizeAngle(minuteClockHand.transform.localEulerAngles.y);

        float correctHourAngle = GetTargetHourAngle();
        float correctMinuteAngle = GetTargetMinuteAngle();

        float hourDiff = Mathf.Abs(hourClockHandAngle - correctHourAngle);
        float minuteDiff = Mathf.Abs(minuteClockHandAngle - correctMinuteAngle);

        return hourDiff < AnswerOffset && minuteDiff < AnswerOffset;
    }

    public override IEnumerator Run()
    {
        float timer = timeLimit;

        while(timer > 0f)
        {
            if (isEnd)
                yield break;

            if(IsCorrectTime())
            {
                CancelAttack();
                BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
                yield break;
            }

            BattleManager.Instance.timeLimitText.text = Mathf.CeilToInt(timer) + "��";
            timer -= Time.deltaTime;
            yield return null;
        }

        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
        ClearClock();
    }

    public override void CancelAttack()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        isEnd = true;
        ClearClock();
    }

    void ClearClock()
    {
        PhotonNetwork.Destroy(hourClockHand);
        PhotonNetwork.Destroy(minuteClockHand);

        hourClockHandUI.GetComponent<Image>().enabled = false;
        minuteClockHandUI.GetComponent<Image>().enabled = false;
        BattleManager.Instance.timeLimitText.GetComponent<TMP_Text>().enabled = false;
    }
}
