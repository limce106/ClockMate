using Photon.Pun;
using System.Collections;
using System.Linq;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockHandRecovery : AttackPattern
{
    [SerializeField] private Image hourClockHandUI;
    [SerializeField] private Image minuteClockHandUI;

    // 맞춰야 할 목표 시간
    private int targetHour;
    private int targetMinute;

    private GameObject hourClockHand;
    private GameObject minuteClockHand;

    private int hourClockHandViewID;
    private int minuteClockHandViewID;

    private const string HourClockHandPrefabPath = "Prefabs/RecoveryHourClockHand";
    private const string MinuteClockHandPrefabPath = "Prefabs/RecoveryMinuteClockHand";
    private const float MinDistance = 10f;   // 분침, 시침 스폰 위치 간의 최소 거리
    private const float AnswerMargin = 10f; // 스폰된 시계 바늘들이 정답과 겹치지 않도록 여유 두기
    private const float AnswerOffset = 3f; // 정답 오차 허용 범위
    private const float SpawnPosY = 0.5f;   // 스폰 위치 Y값

    protected override void Init()
    {
        
    }

    private void Start()
    {
        ShowRandomTargetTime();
        SpawnClockHands();
    }

    /// <summary>
    /// 랜덤으로 정답 시간을 정해서 UI로 띄우기
    /// </summary>
    private void ShowRandomTargetTime()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int randomHour = Random.Range(1, 13);
        int randomMinute = Random.Range(1, 12) * 5;     // 5분 단위로 보여줌

        photonView.RPC(nameof(RPC_SetTargetTime), RpcTarget.All, randomHour, randomMinute);
    }

    [PunRPC]
    private void RPC_SetTargetTime(int hour, int minute)
    {
        targetHour = hour;
        targetMinute = minute;

        hourClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetHourAngle());
        minuteClockHandUI.transform.localEulerAngles = new Vector3(0, 0, -GetTargetMinuteAngle());

        Debug.Log("정답 시간: " + targetHour + "시 " + targetMinute + "분");
    }

    /// <summary>
    /// 시침 정답 각도
    /// </summary>
    float GetTargetHourAngle()
    {
        return (targetHour % 12) * 30f;
    }

    /// <summary>
    /// 분침 정답 각도
    /// </summary>
    float GetTargetMinuteAngle()
    {
        return targetMinute * 6f;
    }

    /// <summary>
    /// 시침, 분침 모델을 랜덤 각도로 스폰
    /// 두 바늘 최소 사이각, 정답으로부터 최소 각도 제한
    /// </summary>
    void SpawnClockHands()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        float targetHourAngle = GetTargetHourAngle();
        float targetMinuteAngle = GetTargetMinuteAngle();

        float randomHourAngle = GetRandomAngleExcluding(targetHourAngle);
        float randomMinuteAngle = GetRandomAngleExcluding(targetMinuteAngle, randomHourAngle);

        Vector3 SpawnPos = BattleManager.Instance.BattleFieldCenter;
        SpawnPos.y = SpawnPosY;

        hourClockHand = PhotonNetwork.Instantiate(HourClockHandPrefabPath, SpawnPos, Quaternion.Euler(0, -randomHourAngle, 0));
        minuteClockHand = PhotonNetwork.Instantiate(MinuteClockHandPrefabPath, SpawnPos, Quaternion.Euler(0, -randomMinuteAngle, 0));

        hourClockHandViewID = hourClockHand.GetComponent<PhotonView>().ViewID;
        minuteClockHandViewID = minuteClockHand.GetComponent<PhotonView>().ViewID;

        photonView.RPC(nameof(RPC_SetClockHandRef), RpcTarget.All, hourClockHandViewID, minuteClockHandViewID);
    }

    [PunRPC]
    private void RPC_SetClockHandRef(int hourID, int minuteID)
    {
        hourClockHandViewID = hourID;
        minuteClockHandViewID = minuteID;

        hourClockHand = PhotonView.Find(hourID)?.gameObject;
        minuteClockHand = PhotonView.Find(minuteID)?.gameObject;
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
    /// 현재 시침, 분침 모델 각도가 정답인지 확인
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
    /// 두 시계 바늘의 사이각을 기준으로 회전 가능 여부 결정
    /// 현재 사용 안 함
    /// </summary>
    public bool CanRotate(IAClockHand hand, int direction)
    {
        if (hourClockHand == null || minuteClockHand == null)
            return false;

        IAClockHand other = hand.gameObject == hourClockHand
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
        while (true)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // 시간 초과 처리
                if (BattleManager.Instance.IsTimeLimitEnd())
                {
                    EndRecovery(false);
                    yield break;
                }

                // 정답 확인 로직은 모든 클라이언트에서 실행
                if (IsCorrectTime())
                {
                    EndRecovery(true);
                    yield break;
                }

                yield return null;
            }
        }
    }

    [PunRPC]
    void RPC_DetachAllPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_ForceExitControls), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_ForceExitControls()
    {
        IAClockHand hour = hourClockHand?.GetComponentInChildren<IAClockHand>();
        hour?.ExitControl();

        IAClockHand minute = minuteClockHand?.GetComponentInChildren<IAClockHand>();
        minute?.ExitControl();
    }

    void EndRecovery(bool isSuccess)
    {
        photonView.RPC(nameof(RPC_DetachAllPlayers), RpcTarget.All);
        
        // 공격 결과 보고
        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, isSuccess);
    }

    public override void CleanUpAttack()
    {
        StartCoroutine(DestroyClockHands());
    }

    private IEnumerator DestroyClockHands()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        // 모든 캐릭터들이 부착 해제될 때까지 대기
        yield return new WaitUntil(() =>
        {
            bool allParentsAreNull = GameManager.Instance.Characters.Values.All(
                character => character.photonView.transform.parent == null
            );

            return allParentsAreNull;
        });

        if (hourClockHand != null)
            PhotonNetwork.Destroy(hourClockHand);
        if (minuteClockHand != null)
            PhotonNetwork.Destroy(minuteClockHand);

        hourClockHand = null;
        minuteClockHand = null;

        cleanUpEnded = true;
    }
}
