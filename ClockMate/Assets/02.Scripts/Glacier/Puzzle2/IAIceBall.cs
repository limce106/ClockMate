using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아워가 밀 수 있는 빙벽.
/// - 빙벽은 구형으로 회전하면서 이동한다.
/// </summary>
public class IAIceBall : MonoBehaviour, IInteractable
{
    [SerializeField] private float moveForce;
    [SerializeField] private float torqueForce;
    [SerializeField] private GameObject iceBallRootGo; // 이동 주체(부모)
    [SerializeField] private Transform controllerPos;
    [SerializeField] private float radiusOffset;
    
    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;
    
    private Rigidbody _rb; // IceBallObj의 rb
    private bool _isControlled;
    private CharacterBase _controller;
    private Vector3 _characterLocalOffset;

    private float _controllerRadius; // 빙벽 반지름 + 여유 거리
    
    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _isControlled = false;
        _controller = null;
        
        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "나가기";

        // 반지름 + 여유거리 계산
        float rawRadius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        _controllerRadius = rawRadius + radiusOffset;
    }
    
    private void FixedUpdate()
    {
        if (!_isControlled) return;

        // 이동 입력 처리
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) input += Vector3.back;
        if (Input.GetKey(KeyCode.A)) input += Vector3.left;
        if (Input.GetKey(KeyCode.D)) input += Vector3.right;

        if (input != Vector3.zero)
        {
            Vector3 dir = input.normalized;
            
            // 빙벽 이동
            iceBallRootGo.transform.position += dir * (moveForce * Time.fixedDeltaTime); 

            // 빙벽 회전
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, dir);
            _rb.AddTorque(torqueAxis * torqueForce, ForceMode.Force);
        }
    }
    
    private void Update()
    {
        if (!_isControlled) return;

        if (_controller is not null)
        {
            MoveController();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitControl();
        }
    }

    private void MoveController()
    {
        Vector3 target = controllerPos.position;
        target.y = _controller.transform.position.y;

        _controller.transform.position = target;
    }

    public bool CanInteract(CharacterBase character)
    {
        return character is Hour && !_isControlled;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        if (character is not Hour hour) return false;

        _isControlled = true;
        _controller = hour;
        SetControllerPos();
        _controller.ChangeState<PushState>(controllerPos.transform);
        _rb.isKinematic = false;

        _controller.InputHandler.enabled = false;
        
        // 그만두기 UI 표시
        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);
        
        // 상호작용 탐지되지 않도록 collider 비활성화
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }

        return true;
    }

    /// <summary>
    /// controllerPos 위치 및 방향 설정
    /// </summary>
    private void SetControllerPos()
    {
        Vector3 dir = (transform.position - _controller.transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();

            // 빙벽 중심 기준 offset 방향으로 controllerPos 위치 이동
            controllerPos.position = transform.position - dir * _controllerRadius;

            // 빙벽을 바라보도록 회전
            controllerPos.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void ExitControl()
    {
        _isControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;
        _controller = null;
        _rb.isKinematic = true;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
        
        // collider 다시 활성화
        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
    }
}
