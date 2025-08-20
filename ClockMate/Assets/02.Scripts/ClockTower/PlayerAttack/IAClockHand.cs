using Photon.Pun;
using UnityEngine;
using static Define.Character;

public class IAClockHand : MonoBehaviour, IInteractable
{
    private ClockHandRecovery _clockHandRecovery;
    private ClockHandController _clockHandController;
    [SerializeField] private CharacterName ControllerName;
    private int _fixedRotationDirection = 0;

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    public MeshRenderer meshRenderer;
    private CharacterBase _controller;
    private bool _isControlled;

    private const float RotationSpeed = 20f;

    private void Awake()
    {
        _clockHandRecovery = FindObjectOfType<ClockHandRecovery>();
        _clockHandController = transform.root.GetComponent<ClockHandController>();

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
                _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_Rotate), RpcTarget.All, _fixedRotationDirection * RotationSpeed * Time.deltaTime);
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

        _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_AttachController), RpcTarget.All, _controller.photonView.ViewID);

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

    public void ExitControl()
    {
        if (_controller == null) return;

        _isControlled = false;
        _controller.ChangeState<IdleState>();
        _controller.InputHandler.enabled = true;

        _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_DetachController), RpcTarget.All, _controller.photonView.ViewID);
        if (_controller.photonView.IsMine)
        {
            _controller = null;
        }

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
    }
}