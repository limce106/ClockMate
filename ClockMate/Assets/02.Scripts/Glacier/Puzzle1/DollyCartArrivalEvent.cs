using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class DollyCartArrivalEvent : MonoBehaviour
{
    [SerializeField] private CinemachineDollyCart cart;
    [SerializeField] private float epsilon = 0.01f; // 판정 여유
    [SerializeField] private bool fireOnce = true;

    public UnityEvent onArrivedEnd;      // 루프x: 트랙 끝 도착
    public UnityEvent onArrivedStart;    // 루프x: 트랙 시작 도착
    public UnityEvent onLapCompleted;    // 루프: 한 바퀴 완료

    private bool _firedEnd;
    private bool _firedStart;
    private float _prevPos;
    private bool _initialized;

    private void Reset()
    {
        cart = GetComponent<CinemachineDollyCart>();
    }

    private void Update()
    {
        if (cart == null || cart.m_Path == null) return;

        // Position Units 최대값 반환
        float maxUnit = cart.m_Path.MaxUnit(cart.m_PositionUnits);
        float pos = cart.m_Position;
        bool looped = cart.m_Path.Looped;

        if (!_initialized)
        {
            _prevPos = pos;
            _initialized = true;
        }

        if (!looped)
        {
            // 진행 방향에 따른 도착 -> 정방향은 End, 역방향은 Start
            if (cart.m_Speed >= 0f)
            {
                if (pos >= maxUnit - epsilon)
                {
                    if (!fireOnce || !_firedEnd)
                    {
                        _firedEnd = true;
                        onArrivedEnd?.Invoke();
                    }
                }
                if (pos <= 0f + epsilon)
                {
                    if (!fireOnce || !_firedStart)
                    {
                        _firedStart = true;
                        onArrivedStart?.Invoke();
                    }
                }
            }
            else // 역방향 주행
            {
                if (pos <= 0f + epsilon)
                {
                    if (!fireOnce || !_firedStart)
                    {
                        _firedStart = true;
                        onArrivedStart?.Invoke();
                    }
                }
                if (pos >= maxUnit - epsilon)
                {
                    if (!fireOnce || !_firedEnd)
                    {
                        _firedEnd = true;
                        onArrivedEnd?.Invoke();
                    }
                }
            }
        }
        else
        {
            float speed = cart.m_Speed;
            bool wrappedForward  = speed >= 0f && pos + epsilon < _prevPos;
            bool wrappedBackward = speed < 0f  && pos - epsilon > _prevPos;

            if (wrappedForward || wrappedBackward)
            {
                onLapCompleted?.Invoke();
            }
        }

        _prevPos = pos;
    }

    /// <summary>
    /// 외부에서 호출 가능한 초기화 메서드
    /// </summary>
    public void ResetFlags()
    {
        _firedEnd = _firedStart = false;
    }
}