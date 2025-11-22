using System;
using System.Collections;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using static Define.Icon;

/// <summary>
/// 아워가 밀 수 있는 빙벽.
/// - 빙벽은 구형으로 회전하면서 이동한다.
/// </summary>
public class IceBall : MonoBehaviourPun
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
    private UIControlHelp _uiHelp;
    
    public bool IsControlled { get; private set; }
    private CharacterBase _controller;
    private Vector3 _characterLocalOffset;
    private Transform _camTransform;

    public Action<bool> OnControlEnd;
    private SoundHandle _soundHandle;
    private bool _sfxPlayed;
    
    private Vector3 _originalPosition;
    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        IsControlled = false;
        _controller = null;
        
        _exitSprite = Key.Q.LoadSprite(Style.Outline);;
        _exitString = "나가기";

        _camTransform = Camera.main.transform;
        _originalPosition = iceBallRootGo.transform.position;
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

            photonView.RPC(nameof(RPC_MoveBall), RpcTarget.All, dir);
            if (_controller != null) MoveController();
        }

    }
    
    [PunRPC]
    private void RPC_MoveBall(Vector3 dir)
    {
        // 이동
        Vector3 position = iceBallRootGo.transform.position + dir * (moveForce * Time.fixedDeltaTime);
        iceBallRootGo.GetComponent<Rigidbody>().MovePosition(position);

        // 모델 회전 처리
        Vector3 torqueAxis = Vector3.Cross(Vector3.up, dir);
        transform.Rotate(torqueAxis, torqueForce * Time.fixedDeltaTime, Space.World);
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
        Vector3 target = new Vector3(controllerPos.position.x, _controller.transform.position.y, controllerPos.position.z);
        target.y = _controller.transform.position.y;

        _controller.transform.position = target;
    }

    public void StartControl(CharacterBase controller)
    {
        IsControlled = true;
        _controller = controller;
        SetControllerPos();
        MoveController();

        Vector3 lookDirection = controllerPos.transform.forward;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        _controller.ChangeState<PushState>(targetRotation);

        _controller.InputHandler.enabled = false;
        StartCoroutine(WaitForAnimTransition());
        
        // 그만두기 UI 표시
        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);
        _uiHelp = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        _uiHelp.SetOnlyFirst(Key.Arrows.LoadSprite(), "움직이기");
        
        GameManager.Instance.CurrentStage.CbExit -= ExitControl;
        GameManager.Instance.CurrentStage.CbExit += ExitControl;
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
        Vector3 dir = _controller.transform.position - iceBallRootGo.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            // 빙벽 중심 기준 offset 방향으로 controllerPos 위치 이동
            controllerPos.position = iceBallRootGo.transform.position + dir * 5f;
                        
            // 빙벽을 바라보도록 회전
            controllerPos.rotation = Quaternion.LookRotation(-dir);
        }
    }

    private void ExitControl()
    {
        if (!IsControlled) return;
        IsControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;
        _controller.Anim.SetPush(false);
        OnControlEnd(true); // 충돌 다시 활성화
        _controller = null;

        _uiHelp.Close();
        _uiHelp = null;
        _uiNotice.Close();
        _uiNotice = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            ExitControl();
            iceBallRootGo.transform.position = _originalPosition;
        }
    }
}
