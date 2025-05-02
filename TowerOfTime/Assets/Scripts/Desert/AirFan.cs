using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirFan : MonoBehaviour
{
    private GameObject player;
    private Rigidbody playerRb;

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

    [Header("환풍기 회전")]
    public bool isFanOn = false;
    private float startRotationSpeed = 0f;
    private float currentRotationSpeed = 0f;
    public const float maxRotationSpeed = 100f;

    private const float rotationTransitionTime = 1f;
    private float rotationElapsedTime = 0f;

    private enum FanState { Idle, SpinningUp, SpinningDown, Running }
    private FanState fanState = FanState.Idle;

    [SerializeField]
    private bool isInFanTrigger = false;
    [SerializeField]
    private bool isFlying = false;
    private bool isUpwardFly = false;
    
    public GameObject fanBlades;
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
                playerRb.useGravity = false;
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
                LerpFanBlades(maxRotationSpeed, FanState.Running);
                break;
            case FanState.SpinningDown:
                LerpFanBlades(0f, FanState.Idle);
                playerRb.useGravity = true;
                break;
        }

        RotateFanBlades();
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
        isFlying = false;
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

        startRotationSpeed = currentRotationSpeed;
        rotationElapsedTime = 0f;
    }

    void LerpFanBlades(float targetSpeed, FanState nextState)
    {
        if (rotationElapsedTime < rotationTransitionTime)
        {
            rotationElapsedTime += Time.deltaTime;
            float transitionRatio = rotationElapsedTime / rotationTransitionTime;
            currentRotationSpeed = Mathf.Lerp(startRotationSpeed, targetSpeed, transitionRatio);
        }
        else
        {
            currentRotationSpeed = targetSpeed;
            fanState = nextState;
        }
    }

    void RotateFanBlades()
    {
        fanBlades.transform.Rotate(Vector3.forward, currentRotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        isInFanTrigger = true;

        if(!isUpwardFly)
        {
            startFlyPoint = player.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isInFanTrigger = false;
    }
}
