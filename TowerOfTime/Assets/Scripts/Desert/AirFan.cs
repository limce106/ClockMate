using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirFan : MonoBehaviour
{
    private GameObject player;
    private Rigidbody playerRb;

    // 바람 세기
    private float windForce = 2f;
    // 바람의 최대 범위
    private float windRange = 10f;
    public bool isFanOn = false;
    //public ParticleSystem windEffect;

    public Transform fanBlades;

    private float startRotationSpeed = 0f;
    private float currentRotationSpeed = 0f;
    public const float maxRotationSpeed = 100f;

    private const float rotationTransitionTime = 1f;
    private float rotationElapsedTime = 0f;

    private enum FanState { Idle, SpinningUp, SpinningDown, Running }
    private FanState fanState = FanState.Idle;

    float maxHeight;

    void Start()
    {
        maxHeight = transform.position.y + windRange;

        player = GameObject.Find("Milli");
        if (player != null )
        {
            playerRb = player.GetComponent<Rigidbody>();
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
                ApplyWindForce();
                break;
            case FanState.SpinningDown:
                LerpFanBlades(0f, FanState.Idle);
                playerRb.useGravity = true;
                break;
            case FanState.Running:
                ApplyWindForce();
                break;
        }

        RotateFanBlades();
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

    void ApplyWindForce()
    {
        if (player == null)
            return;

        Vector3 direction = transform.up;
        Vector3 fanToPlayer = player.transform.position - transform.position;
        float distance = Vector3.Dot(fanToPlayer, direction.normalized);

        if(distance <= windRange)
        {
            float windStrength = Mathf.Lerp(0, windForce, currentRotationSpeed / maxRotationSpeed);
            float forceMultiplier = 1 - (distance / windRange);
            Vector3 force = direction * windStrength * forceMultiplier;

            playerRb.useGravity = false;

            float playerHeight = player.transform.position.y;

            if (playerHeight < maxHeight)
            {
                playerRb.AddForce(force, ForceMode.Acceleration);
            }
            else
            {
                playerRb.velocity = Vector3.zero;
                player.transform.position = new Vector3(player.transform.position.x, maxHeight, player.transform.position.z);
            }
        }
        else
        {
            player.transform.position = new Vector3(player.transform.position.x, maxHeight, player.transform.position.z);
        }
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
        fanBlades.Rotate(Vector3.forward, currentRotationSpeed * Time.deltaTime);
    }
}
