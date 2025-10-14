using DefineExtension;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;

public abstract class ClimbObjectBase : MonoBehaviourPun, IInteractable
{
    protected UIManager _uiManager;
    protected UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;
    protected UIClimbableObj _uiClimbableObj;

    protected bool isColliding = false;
    public bool playerAttached = false;
    private SoundHandle _climbSfxHandle = default;

    [SerializeField] protected Transform topPoint;
    [SerializeField] protected Transform bottomPoint;
    [SerializeField] protected Transform topTargetPoint;
    public Transform TopTargetPoint => topTargetPoint;

    public float topY { private set; get; }
    public float bottomY { private set; get; }

    protected virtual void Awake()
    {
        _uiManager = UIManager.Instance;

        topY = topPoint.position.y;
        bottomY = bottomPoint.position.y;

        _exitSprite = Icon.Key.Q.LoadSprite(Icon.Style.Outline);;
        _exitString = "그만타기";
    }

    /// <summary>
    /// 조작키 UI 없애기
    /// </summary>
    public void CloseUI()
    {
        if(_uiClimbableObj != null && _uiClimbableObj.gameObject.activeSelf)
            _uiManager.Close(_uiClimbableObj);
        if (_uiNotice != null && _uiNotice.gameObject.activeSelf)
            _uiManager.Close(_uiNotice);
    }

    public virtual bool CanInteract(CharacterBase character)
    {
        if(playerAttached)
            return false;

        if (character.CurrentState is ClimbState)
            return false;

        if (!character.IsGrounded)
            return false;

        if (isColliding)
            return true;
        else
            return false;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable() { }

    public bool Interact(CharacterBase character)
    {
        if (character.photonView.IsMine)
        {
            character.photonView.RPC("RPC_StartClimbing", RpcTarget.All, GetComponent<PhotonView>().ViewID);
        }

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        _uiClimbableObj = _uiManager.Show<UIClimbableObj>("UIClimbableObj");

        EnableColliders(false);

        return true;
    }

    public void EnableColliders(bool enable)
    {
        Collider collider = GetComponent<Collider>();
        collider.enabled = enable;
    }

    public abstract void AttachTo(CharacterBase character);

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    [PunRPC]
    public void RPC_StartClimbSfx()
    {
        if (!_climbSfxHandle.IsValid)
        {
            _climbSfxHandle = SoundManager.Instance.PlaySfx(
                key: "character_climb",
                loop: true,
                pos: transform.position,
                sync: false,
                volume: 1f);
        }
    }

    [PunRPC]
    public void RPC_StopClimbSfx()
    {
        if (_climbSfxHandle.IsValid)
        {
            SoundManager.Instance.Stop(_climbSfxHandle);
            _climbSfxHandle = default;
        }
    }
}
