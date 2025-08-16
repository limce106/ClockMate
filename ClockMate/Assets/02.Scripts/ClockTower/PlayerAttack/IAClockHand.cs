using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class IAClockHand : MonoBehaviourPun, IInteractable
{
    private ClockHandRecovery _clockHandRecovery;
    [SerializeField] private CharacterName ControllerName;
    private int _fixedRotationDirection = 0;

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    public MeshRenderer meshRenderer;
    private Rigidbody _rb;
    private CharacterBase _controller;
    private bool _isControlled;

    private const float ControllerOffset = 1.2f;
    private const float RotationSpeed = 20f;

    private void Awake()
    {
        _clockHandRecovery = FindObjectOfType<ClockHandRecovery>();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "나가기";
    }

    void Update()
    {
        // 조작 중인 플레이어가 로컬 플레이어인지 확인
        bool isLocalPlayerControlling = _isControlled && _controller != null && _controller.photonView.IsMine;

        if (isLocalPlayerControlling)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitControl();
                return;
            }

            if (Input.GetKey(KeyCode.W) && _fixedRotationDirection != 0)
            {
                photonView.RPC(nameof(RPC_Rotate), RpcTarget.All, _fixedRotationDirection * RotationSpeed * Time.deltaTime);
            }
        }
    }

    public bool CanInteract(CharacterBase character)
    {
        if (_isControlled) return false;
        if (character.Name != ControllerName) return false;
        if (character.transform.position.y >= transform.position.y) return false;

        return true;
    }

    public void OnInteractAvailable() { }
    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        if (GetDirectionFromView(character) == 0) return false;

        _isControlled = true;
        _controller = character;
        _controller.ChangeState<PushState>(meshRenderer.transform);
        _controller.InputHandler.enabled = false;

        _fixedRotationDirection = GetDirectionFromView(character);

        photonView.RPC(nameof(RPC_AttachController), RpcTarget.All, _controller.photonView.ViewID);

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        return true;
    }

    private int GetDirectionFromView(CharacterBase character)
    {
        Vector3 meshForward = meshRenderer.transform.forward;
        Vector3 playerDir = character.transform.position - meshRenderer.transform.position;
        float crossY = Vector3.Cross(meshForward, playerDir).y;
        if (crossY > 0) return -1;
        else if (crossY < 0) return 1;
        else return 0;
    }

    private void ExitControl()
    {
        _isControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;

        photonView.RPC(nameof(RPC_DetachController), RpcTarget.All, _controller.photonView.ViewID);

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
    }

    [PunRPC]
    private void RPC_AttachController(int controllerViewID)
    {
        PhotonView controllerView = PhotonView.Find(controllerViewID);
        if (controllerView == null) return;

        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] controllerColliders = controllerView.GetComponents<Collider>();

        foreach (var controllerCol  in controllerColliders)
        {
            foreach (var handCol in handColliders)
            {
                // 시계 바늘과 플레이어의 충돌 무시
                Physics.IgnoreCollision(controllerCol, handCol, true);
            }
        }

        foreach (var handCol in handColliders)
        {
            handCol.enabled = false;
        }

        controllerView.GetComponent<Rigidbody>().isKinematic = true;

        PhotonTransformView photonTransformView = controllerView.GetComponent<PhotonTransformView>();
        photonTransformView.enabled = false;

        _rb.isKinematic = false;

        controllerView.transform.SetParent(meshRenderer.transform);

        float originControllerY = controllerView.transform.position.y;
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 toPlayer = controllerView.transform.position - transform.position;
        float side = Vector3.Dot(toPlayer, right);
        Vector3 attachDir = side >= 0 ? right : -right;

        Vector3 targetPos = transform.position + attachDir * ControllerOffset;
        targetPos.y = originControllerY;

        controllerView.transform.position = targetPos;
        controllerView.transform.rotation = Quaternion.LookRotation(-attachDir);
    }

    [PunRPC]
    private void RPC_DetachController(int controllerViewID)
    {
        PhotonView controllerView = PhotonView.Find(controllerViewID);
        if (controllerView == null) return;

        if (controllerView.IsMine)
        {
            _controller = null;
        }

        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] controllerColliders = controllerView.GetComponents<Collider>();

        foreach (var controllerCol in controllerColliders)
        {
            foreach (var handCol in handColliders)
            {
                Physics.IgnoreCollision(controllerCol, handCol, false);
            }
        }

        foreach (var handCol in handColliders)
        {
            handCol.enabled = true;
        }

        controllerView.GetComponent<Rigidbody>().isKinematic = false;
        _rb.isKinematic = true;

        controllerView.transform.SetParent(null);
        controllerView.GetComponent<PhotonTransformView>().enabled = true;

        //RaycastHit hit;
        //if (Physics.Raycast(controllerView.transform.position + Vector3.up, Vector3.down, out hit, 5f))
        //{
        //    controllerView.transform.position = hit.point + Vector3.up * 0.01f;
        //    controllerView.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * controllerView.transform.rotation;
        //}
    }

    [PunRPC]
    private void RPC_Rotate(float rotationAmount)
    {
        transform.root.Rotate(0f, rotationAmount, 0f);
    }
}