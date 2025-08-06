using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockNeedleRecovery : AttackPattern
{
    [SerializeField] private Image hourNeedleUI;
    [SerializeField] private Image minuteNeedleUI;

    // ����� �� ��ǥ �ð�
    private int targetHour;
    private int targetMinute;

    private GameObject hourNeedle;
    private GameObject minuteNeedle;

    private bool isEnd = false;

    private const float timeLimit = 30f;

    private const string HourNeedlePrefabPath = "Prefabs/RecoveryHourNeedle";
    private const string MinuteNeedlePrefabPath = "Prefabs/RecoveryMinuteNeedle";
    private const float MinDistance = 5f;   // ��ħ, ��ħ ���� ��ġ ���� �ּ� �Ÿ�
    private const float AnswerMargin = 10f; // ������ �ð� �ٴõ��� ����� ��ġ�� �ʵ��� ���� �α�
    private const float AnswerOffset = 3f; // ���� ���� ��� ����

    protected override void Init()
    {
        ShowRandomTargetTime();
        SpawnNeedles();
    }

    private void ShowRandomTargetTime()
    {
        targetHour = Random.Range(1, 13);
        targetMinute = Random.Range(1, 12) * 5;     // 5�� ������ ������

        hourNeedleUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetHourAngle());
        minuteNeedleUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetMinuteAngle());

        hourNeedleUI.GetComponent<Image>().enabled = true;
        minuteNeedleUI.GetComponent<Image>().enabled = true;
        BattleManager.Instance.timeLimitText.GetComponent<TMP_Text>().enabled = true;
    }

    float GetTargetHourAngle()
    {
        return (targetHour % 12) * 30f + (targetMinute / 60f) * 30f;
    }

    float GetTargetMinuteAngle()
    {
        return targetMinute * 6f;
    }

    void SpawnNeedles()
    {
        float targetHourAngle = GetTargetHourAngle();
        float targetMinuteAngle = GetTargetMinuteAngle();

        float randomHourAngle = GetRandomAngleExcluding(targetHourAngle);
        float randomMinuteAngle = GetRandomAngleExcluding(targetHourAngle, randomHourAngle);

        hourNeedle = PhotonNetwork.Instantiate(HourNeedlePrefabPath, BattleManager.Instance.BattleFieldCenter, Quaternion.Euler(0, 0, -randomHourAngle));
        minuteNeedle = PhotonNetwork.Instantiate(MinuteNeedlePrefabPath, BattleManager.Instance.BattleFieldCenter, Quaternion.Euler(0, 0, -randomMinuteAngle));
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
        float hourNeedleAngle = NormalizeAngle(hourNeedle.transform.localEulerAngles.z);
        float minuteNeedleAngle = NormalizeAngle(minuteNeedle.transform.localEulerAngles.z);

        float correctHourAngle = GetTargetHourAngle();
        float correctMinuteAngle = GetTargetMinuteAngle();

        float hourDiff = Mathf.Abs(hourNeedleAngle - correctHourAngle);
        float minuteDiff = Mathf.Abs(minuteNeedleAngle - correctMinuteAngle);

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

        PhotonNetwork.Destroy(hourNeedle);
        PhotonNetwork.Destroy(minuteNeedle);

        ClearClock();
    }

    void ClearClock()
    {
        hourNeedleUI.GetComponent<Image>().enabled = false;
        minuteNeedleUI.GetComponent<Image>().enabled = false;
        BattleManager.Instance.timeLimitText.GetComponent<TMP_Text>().enabled = false;
    }
}
