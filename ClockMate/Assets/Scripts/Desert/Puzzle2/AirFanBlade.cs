using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirFanBlade : MonoBehaviour
{
    [HideInInspector]
    public float startRotationSpeed = 0f;
    [HideInInspector]
    public float currentRotationSpeed = 0f;
    public const float maxRotationSpeed = 100f;

    private const float _rotationTransitionTime = 1f;
    private float _rotationElapsedTime = 0f;

    [SerializeField]
    private AirFan airFan;

    void Update()
    {
        RotateFanBlades();
    }

    public void LerpFanBlades(float targetSpeed, AirFan.FanState nextState)
    {
        if (_rotationElapsedTime < _rotationTransitionTime)
        {
            _rotationElapsedTime += Time.deltaTime;
            float transitionRatio = _rotationElapsedTime / _rotationTransitionTime;
            currentRotationSpeed = Mathf.Lerp(startRotationSpeed, targetSpeed, transitionRatio);
        }
        else
        {
            currentRotationSpeed = targetSpeed;
            airFan.SetFanState(nextState);
        }
    }

    void RotateFanBlades()
    {
        transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
    }

    public void ClearRotationElapsedTime()
    {
        _rotationElapsedTime = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(airFan.isFanOn && collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
        }
    }
}