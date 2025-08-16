using Photon.Pun;
using System.Collections;
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
    // ������ Ŭ���̾�Ʈ�� �����ϴ� Ÿ�̸�
    private float _timer;

    private bool isEnd = false;

    private const float timeLimit = 30f;

    private const string HourClockHandPrefabPath = "Prefabs/RecoveryHourClockHand";
    private const string MinuteClockHandPrefabPath = "Prefabs/RecoveryMinuteClockHand";
    private const float MinDistance = 10f;   // ��ħ, ��ħ ���� ��ġ ���� �ּ� �Ÿ�
    private const float AnswerMargin = 10f; // ������ �ð� �ٴõ��� ����� ��ġ�� �ʵ��� ���� �α�
    private const float AnswerOffset = 3f; // ���� ���� ��� ����
    private const float SpawnPosY = 1f;   // ���� ��ġ Y��

    protected override void Init()
    {
        
    }

    private void Start()
    {
        ShowRandomTargetTime();
        SpawnClockHands();

        Debug.Log("Hour: " + GetTargetHourAngle());
        Debug.Log("Minute: " + GetTargetMinuteAngle());
    }

    /// <summary>
    /// �������� ���� �ð��� ���ؼ� UI�� ����
    /// </summary>
    private void ShowRandomTargetTime()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int randomHour = Random.Range(1, 13);
        int randomMinute = Random.Range(1, 12) * 5;     // 5�� ������ ������

        photonView.RPC(nameof(RPC_SetTargetTime), RpcTarget.All, randomHour, randomMinute);
    }

    [PunRPC]
    private void RPC_SetTargetTime(int hour, int minute)
    {
        targetHour = hour;
        targetMinute = minute;

        hourClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetHourAngle());
        minuteClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetMinuteAngle());

        hourClockHandUI.GetComponent<Image>().enabled = true;
        minuteClockHandUI.GetComponent<Image>().enabled = true;
        BattleManager.Instance.timeLimitText.GetComponent<TMP_Text>().enabled = true;
    }

    /// <summary>
    /// ��ħ ���� ����
    /// </summary>
    float GetTargetHourAngle()
    {
        return (targetHour % 12) * 30f;
    }

    /// <summary>
    /// ��ħ ���� ����
    /// </summary>
    float GetTargetMinuteAngle()
    {
        return targetMinute * 6f;
    }

    /// <summary>
    /// ��ħ, ��ħ ���� ���� ������ ����
    /// �� �ٴ� �ּ� ���̰�, �������κ��� �ּ� ���� ����
    /// </summary>
    void SpawnClockHands()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

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

    /// <summary>
    /// ���� ��ħ, ��ħ �� ������ �������� Ȯ��
    /// </summary>
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

    /// <summary>
    /// �� �ð� �ٴ��� ���̰��� �������� ȸ�� ���� ���� ����
    /// </summary>
    public bool CanRotate(IAClockHand hand, int direction)
    {
        if (hourClockHand == null || minuteClockHand == null)
            return false;

        IAClockHand other = hand == hourClockHand
        ? minuteClockHand.GetComponentInChildren<IAClockHand>()
        : hourClockHand.GetComponentInChildren<IAClockHand>();

        Vector3 curForward = hand.meshRenderer.transform.forward;
        Vector3 otherForward = other.meshRenderer.transform.forward;

        float angle = Vector3.Angle(curForward, otherForward);

        if (angle > 10f)
            return true;

        Vector3 cross = Vector3.Cross(curForward, otherForward);
        if(cross.y * direction < 0)
            return true;

        return false;
    }

    public override IEnumerator Run()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _timer = timeLimit;
        }

        while (true)
        {
            if (isEnd)
            {
                yield break;
            }

            // ������ Ŭ���̾�Ʈ�� Ÿ�̸Ӹ� ������Ʈ�ϰ� ����ȭ RPC ȣ��
            if (PhotonNetwork.IsMasterClient)
            {
                _timer -= Time.deltaTime;
                photonView.RPC(nameof(RPC_UpdateTimeLimitTxt), RpcTarget.All, Mathf.CeilToInt(_timer));

                if (_timer <= 0f)
                {
                    // �ð� �ʰ� ó��
                    BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, false);
                    ClearClock();
                    yield break;
                }
            }

            // ���� Ȯ�� ������ ��� Ŭ���̾�Ʈ���� ����
            if (IsCorrectTime())
            {
                // ������ ������ ��
                yield return new WaitForSeconds(2f);
                CancelAttack();
                BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, true);
                yield break;
            }

            yield return null;
        }
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

    [PunRPC]
    private void RPC_UpdateTimeLimitTxt(int time)
    {
        BattleManager.Instance.timeLimitText.text = time + "��";
    }
}
