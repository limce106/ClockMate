using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Vector3 openAxis; // 회전 목표 EulerAngles 
    [SerializeField] private Vector3 openPos; // 이동 목표 
    [SerializeField] private float openSpeed; // 열리는 속도 (degrees/sec)
    [SerializeField] private float closeSpeed;
    [SerializeField] private string sfxKey;
    [SerializeField] private float sfxVolume;

    private Coroutine _openCoroutine;
    private Coroutine _closeCoroutine;
    private Vector3 _startAxis;
    private Vector3 _startPos;
    private SoundHandle _sfxHandle;

    [field: SerializeField] public bool IsLocked { get; private set; }

    private void Awake()
    {
        _startAxis = transform.localRotation.eulerAngles;
        _startPos = transform.localPosition;
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
        Vector3 targetPos = isOpening ? openPos : _startPos;
        SoundManager.Instance.Stop(_sfxHandle); // 재생 중이던 효과음이 있다면 중단

        if (transform.localRotation == targetRotation && transform.localPosition == targetPos) yield break;
        _sfxHandle = SoundManager.Instance.PlaySfx(key: sfxKey, pos: transform.position, volume: sfxVolume);

        const float rotEps = 0.1f;
        const float posEps = 0.001f;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > rotEps
               || Vector3.Distance(transform.localPosition, targetPos) > posEps)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, targetRotation, speed * Time.deltaTime);

            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, targetPos, speed * Time.deltaTime);

            yield return null;
        }

        transform.localRotation = targetRotation;
        transform.localPosition = targetPos;

        if (isOpening) _openCoroutine = null;
        else _closeCoroutine = null;
        SoundManager.Instance.Stop(_sfxHandle); // 효과음 중단
    }

}
