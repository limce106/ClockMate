using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirFan : MonoBehaviour
{
    private GameObject player;
    private Rigidbody playerRb;
    [SerializeField]
    private AirFanBlade fanBlades;

    [Header("수직으로 날려보내기")]
    // 바람 세기
    [SerializeField]
    private float windForce = 10f;
    // 바람의 최대 범위
    [SerializeField]
    private float windRange = 5f;

    [Header("대각선으로 날려보내기")]
    private Transform startFlyPoint;
    [SerializeField, Tooltip("대각선 바람일 때 목표 위치를 설정하세요.")]
    private Transform targetFlyPoint;
    private const float gravity = 9.8f;

    public bool isFanOn = false;
    public enum FanState { Idle, SpinningUp, SpinningDown, Running }
    [HideInInspector]
    public FanState fanState = FanState.Idle;

    [SerializeField]
    private bool isInFanTrigger = false;
    [SerializeField]
    private bool isFlying = false;
    private bool isUpwardFly = false;

    //public ParticleSystem windEffect;

    void Start()
    {
        player = GameObject.Find("Milli");
        if (player != null )
        {
            playerRb = player.GetComponent<Rigidbody>();

            if (Mathf.Approximately(transform.rotation.eulerAngles.x, 0f))
            {
                isUpwardFly = true;
            }
            else
            {
                isUpwardFly = false;
                
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) 
        {
            SwitchFan();
        }

        switch(fanState)
        {
            case FanState.SpinningUp:
                fanBlades.LerpFanBlades(AirFanBlade.maxRotationSpeed, FanState.Running);
                break;
            case FanState.SpinningDown:
                fanBlades.LerpFanBlades(0f, FanState.Idle);
                break;
        }
    }

    void FixedUpdate()
    {
        if (fanState != FanState.Running)
            return;

        if (isUpwardFly)
        {
            if((!isFlying && isInFanTrigger) || isFlying)
            {
                LaunchPlayerUpward();
            }
        }
        else
        {
            if(!isFlying && isInFanTrigger)
            {
                StartCoroutine(LaunchPlayerParabola());
            }
        }
    }

    private void LaunchPlayerUpward()
    {
        float currentY = playerRb.position.y;
        float targetY = transform.position.y + windRange;
        float deltaY = targetY - currentY;

        if (deltaY > 0.05f)
        {
            float forceRatio = Mathf.Clamp01(deltaY / windRange);
            float upwardForce = forceRatio * windForce;
            playerRb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
        else
        {
            // 목표 높이에 도달하면 둥둥 떠 있는 움직임 적용
            float hoverOffset = Mathf.Sin(Time.time * 1f * 2 * Mathf.PI) * 0.5f;
            Vector3 hoverPosition = new Vector3(playerRb.position.x, targetY + hoverOffset, playerRb.position.z);
            playerRb.MovePosition(hoverPosition);
        }
    }

    private IEnumerator LaunchPlayerParabola()
    {
        isFlying = true;
        playerRb.useGravity = false;

        float distance = Vector3.Distance(startFlyPoint.position, targetFlyPoint.position);
        float flyAngle = 90f - transform.rotation.eulerAngles.x;

        float veloctiy = distance / (Mathf.Sin(2 * flyAngle * Mathf.Deg2Rad) / gravity);
        float Vx = Mathf.Sqrt(veloctiy) * Mathf.Cos(flyAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(veloctiy) * Mathf.Sin(flyAngle * Mathf.Deg2Rad);

        float flightDuration = distance / Vx;
        Quaternion targetRotation = Quaternion.LookRotation(targetFlyPoint.position - startFlyPoint.position);

        Vector3 startPosition = player.transform.position;
        float elapseTime = 0f;
        while (elapseTime < flightDuration)
        {
            if(!isFanOn)
            {
                break;
            }

            // 플레이어가 수동 조작으로 움직인 경우 낙하
            if (playerRb.velocity.magnitude  > 0.1f)
            {
                Debug.Log("Player moved: Cancel Flying");

                EndFlying();

                yield break;
            }

            float timeRatio = elapseTime / flightDuration;

            float yOffset = (Vy * elapseTime) - (0.5f * gravity * elapseTime * elapseTime);
            Vector3 horizontalMovement = Vector3.forward * Vx * elapseTime;
            Vector3 newPosition = startPosition + player.transform.TransformDirection(horizontalMovement) + Vector3.up * yOffset;

            player.transform.position = newPosition;
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * 5f);

            elapseTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetFlyPoint.position;
        EndFlying();
    }

    private void EndFlying()
    {
        isFlying = false;
        playerRb.useGravity = true;
    }

    public void SwitchFan()
    {
        if(isFanOn)
        {
            isFanOn = false;
            fanState = FanState.SpinningDown;
            //windEffect.Stop();
        }
        else
        {
            isFanOn = true;
            fanState = FanState.SpinningUp;
            //windEffect.Play();
        }

        fanBlades.startRotationSpeed = fanBlades.currentRotationSpeed;
        fanBlades.ClearRotationElapsedTime();
    }

    public void SetFanState(FanState nextState)
    {
        fanState = nextState;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.name == "Milli")
        {
            isInFanTrigger = true;

            if (!isUpwardFly)
            {
                startFlyPoint = player.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Milli")
        {
            isInFanTrigger = false;
        }
    }
}
