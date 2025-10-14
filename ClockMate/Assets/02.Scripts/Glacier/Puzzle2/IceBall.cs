using System;
using System.Collections;
using Define;
using DefineExtension;
using UnityEngine;

/// <summary>
/// 아워가 밀 수 있는 빙벽.
/// - 빙벽은 구형으로 회전하면서 이동한다.
/// </summary>
public class IceBall : MonoBehaviour
{
    [SerializeField] private float moveForce;
    [SerializeField] private float torqueForce;
    [SerializeField] private GameObject iceBallRootGo; // 이동 주체(부모)
    [SerializeField] private Transform controllerPos;
    [SerializeField] private float radiusOffset;
    [SerializeField] private string rollSfxKey;
    
    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;
    
    public bool IsControlled { get; private set; }
    private CharacterBase _controller;
    private Vector3 _characterLocalOffset;
    private SphereCollider _sphereCollider;
    private Transform _camTransform;

    private float _controllerRadius; // 빙벽 반지름 + 여유 거리
    public Action<bool, CharacterBase> OnControlEnd;
    
    private SoundHandle _soundHandle;
    private bool _sfxPlayed;
    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        IsControlled = false;
        _controller = null;
        
        _exitSprite = Icon.Key.Q.LoadSprite(Icon.Style.Outline);;
        _exitString = "나가기";

        // 반지름 + 여유거리 계산
        _sphereCollider = GetComponent<SphereCollider>();
        float rawRadius = _sphereCollider.radius * transform.localScale.x;
        _controllerRadius = rawRadius + radiusOffset;
        _camTransform = Camera.main.transform;
    }
    
    private void FixedUpdate()
    {
        if (!IsControlled) return;

        // 이동 입력 처리
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;

        Vector3 camFwd = Vector3.ProjectOnPlane(_camTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_camTransform.right,  Vector3.up).normalized;

        Vector3 dir = (camFwd * v + camRight * h);
        if (dir.sqrMagnitude > 1e-6f)
        {
            dir.Normalize();

            // 이동
            iceBallRootGo.transform.position += dir * (moveForce * Time.fixedDeltaTime);

            // 모델 회전 처리
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, dir);
            transform.Rotate(torqueAxis, torqueForce * Time.fixedDeltaTime, Space.World);

            if (_controller != null) MoveController();
        }

    }
    
    private void Update()
    {
        if (!IsControlled) return;
        
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

    public void StartControl(CharacterBase controller)
    {
        IsControlled = true;
        _controller = controller;
        SetControllerPos();
        MoveController();
        _controller.ChangeState<PushState>(controllerPos.transform);

        _controller.InputHandler.enabled = false;
        StartCoroutine(WaitForAnimTransition());
        
        // 그만두기 UI 표시
        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);
    }

    private IEnumerator WaitForAnimTransition()
    {
        yield return new WaitForSeconds(0.2f);
        _controller.Anim.SetPush(true);
    }
    /// <summary>
    /// controllerPos 위치 및 방향 설정
    /// </summary>
    private void SetControllerPos()
    {
        Vector3 dir = (iceBallRootGo.transform.position - _controller.transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            // 빙벽 중심 기준 offset 방향으로 controllerPos 위치 이동
            controllerPos.position = iceBallRootGo.transform.position - dir * _controllerRadius;

            // 빙벽을 바라보도록 회전
            controllerPos.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void ExitControl()
    {
        IsControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;
        _controller.Anim.SetPush(false);
        OnControlEnd(false, _controller); // 충돌 다시 활성화
        _controller = null;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
    }
}
