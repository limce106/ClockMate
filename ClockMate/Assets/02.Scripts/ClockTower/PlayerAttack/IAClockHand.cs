using Define;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using static Define.Character;
using static Define.Icon;

public class IAClockHand : MonoBehaviour, IInteractable
{
    private ClockHandRecovery _clockHandRecovery;
    private ClockHandController _clockHandController;
    [SerializeField] private CharacterName ControllerName;
    private int _fixedRotationDirection = 0;

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;
    private UIControlHelp _uiControlHelp;

    public MeshRenderer meshRenderer;
    private CharacterBase _controller;
    private bool _isControlled;

    private const float RotationSpeed = 20f;

    private void Awake()
    {
        _clockHandRecovery = FindObjectOfType<ClockHandRecovery>();
        _clockHandController = transform.root.GetComponent<ClockHandController>();

        _exitSprite = Icon.Key.Q.LoadSprite(Icon.Style.Outline);
        _exitString = "나가기";
    }

    private void Update()
    {
        if (!_isControlled) return;
        if (_controller != null && !_controller.photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitControl();
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_PlayPushClockHandSfx), RpcTarget.All);
            _controller.Anim.SetPush(true);
        }
        else if(Input.GetKeyUp(KeyCode.W))
        {
            _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_StopPushClockHandSfx), RpcTarget.All);
            _controller.Anim.SetPush(false);
        }
    }

    void FixedUpdate()
    {
        if (!_isControlled) return;

        if (Input.GetKey(KeyCode.W) && _fixedRotationDirection != 0)
        {
            _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_Rotate), RpcTarget.All, _fixedRotationDirection * RotationSpeed * Time.fixedDeltaTime);
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
        _uiNotice.SetVerticalPos(false);
        ActivateHelpUI();

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
        _controller.Anim.SetPush(false);

        _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_DetachController), RpcTarget.All, _controller.photonView.ViewID);
        if (_controller.photonView.IsMine)
        {
            _controller = null;
        }

        _clockHandController.photonView.RPC(nameof(_clockHandController.RPC_StopPushClockHandSfx), RpcTarget.All);

        UIManager.Instance.Close(_uiControlHelp);
        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;
    }

    private void ActivateHelpUI()
    {
        _uiControlHelp = UIManager.Instance.Show<UIControlHelp>("UIControlHelp");
        _uiControlHelp.SetOnlyFirst(Key.W.LoadSprite(Style.Outline), "밀기");
    }
}