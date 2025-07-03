using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Vector3 openAxis; // 회전 목표 EulerAngles 
    [SerializeField] private float openSpeed; // 초당 회전 속도 (degrees/sec)
    [SerializeField] private float closeSpeed; 

    private Coroutine _openCoroutine;
    private Coroutine _closeCoroutine;
    private Vector3 _startAxis;

    [field: SerializeField] public bool IsLocked { get; private set; }

    private void Awake()
    {
        _startAxis = transform.localRotation.eulerAngles;
    }
    
    public void Open()
    {
        if (_openCoroutine != null) return;

        if (_closeCoroutine != null)
        {
            StopCoroutine(_closeCoroutine);
            _closeCoroutine = null;
        }
        _openCoroutine = StartCoroutine(MoveRoutine(Quaternion.Euler(openAxis), true));
    }

    public void Close()
    {
        if (_closeCoroutine != null) return;

        if (_openCoroutine != null)
        {
            StopCoroutine(_openCoroutine);
            _openCoroutine = null;
        }
        _closeCoroutine = StartCoroutine(MoveRoutine(Quaternion.Euler(_startAxis), false));
    }
    
    public void Lock()
    {
        IsLocked = true;
        Close();
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    private IEnumerator MoveRoutine(Quaternion targetRotation, bool isOpening)
    {
        float speed = isOpening ? openSpeed : closeSpeed;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRotation,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.localRotation = targetRotation;

        if (isOpening)
            _openCoroutine = null;
        else
            _closeCoroutine = null;
    }

}
