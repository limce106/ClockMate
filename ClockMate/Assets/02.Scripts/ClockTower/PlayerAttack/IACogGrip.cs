using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static Define.Character;

[RequireComponent(typeof(PhotonView))]
public class IACogGrip : MonoBehaviourPun, IInteractable
{
    public enum GripSide { A, B }

    [Header("Binding")]
    public Cog cog;
    public GripSide side;

    private Collider _col;
    public bool IsOccupied { get; private set; }
    public int HolderViewId { get; private set; }
    private CharacterBase _holder;
    private Rigidbody _holderRb;
    private UINotice _uiNotice;
    private Sprite _dropSprite;
    private string _dropString;

    // 동기화용
    private Vector3 _targetPos;
    private int _anchorSeq = 0;
    private int _lastSeq = -1;
    
    private static readonly HashSet<int> HeldCharacterIds = new HashSet<int>();
    public Action<bool> OnGripStateChanged;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _col = GetComponent<Collider>();
        if (_col == null)
        {
            _col = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)_col).radius = 0.3f;
            _col.isTrigger = true;
        }

        IsOccupied = false;
        HolderViewId = -1;
        _dropSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _dropString = "내려놓기";
    }

    private void Update()
    {
        if (!IsOccupied) return;

        if (_holder.photonView.IsMine && Input.GetKeyDown(KeyCode.Q))
        {
            // 내려놓기 
            Release();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOccupied) return;

        if (PhotonNetwork.IsMasterClient)
        {
            _anchorSeq++;
            photonView.RPC(nameof(RPC_GripAnchorTarget), RpcTarget.All, _anchorSeq, transform.position);
        }
        if (_holder == null || !_holder.photonView.IsMine) return;
        UpdateAttachedPoseLocal();
    }

    public bool CanInteract(CharacterBase character)
    {
        if (!cog || !character) return false;
        // 플레이어가 다른 grip을 잡고있지 않고 해당 grip을 상대가 차지하지 않아야 true
        return !HeldCharacterIds.Contains(character.photonView.ViewID) && !IsOccupied;
    }

    public bool Interact(CharacterBase character)
    {
        photonView.RPC(nameof(RPC_SetGrabState), RpcTarget.All, true, character.photonView.ViewID);
        // LockLocalCharacter(_holder, true);
        // EnableUI(true);
        return true;
    }

    private void EnableUI(bool enable)
    {
        if (enable)
        {
            _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
            _uiNotice.SetImage(_dropSprite);
            _uiNotice.SetText(_dropString);
        } else if (_uiNotice != null)
        {
            UIManager.Instance.Close(_uiNotice);
            _uiNotice = null;
        }
        
    }

    private void LockLocalCharacter(CharacterBase character, bool lockOn)
    {
        if (!character.photonView.IsMine) return;
        character.InputHandler.enabled = !lockOn;
        if (lockOn)
        {
            // 바라보는 방향: 톱니 중심 향하도록 스냅
            Vector3 toCog = cog.transform.position - character.transform.position;
            toCog.y = 0f;
            if (toCog.sqrMagnitude > 0.01f)
            {
                character.transform.rotation = Quaternion.LookRotation(toCog.normalized, Vector3.up);        
            }
            UpdateAttachedPoseLocal();
        }
    }

    /// <summary>
    /// 상호작용하는 캐릭터와 톱니바퀴의 충돌 여부를 설정한다.
    /// </summary>
    private void SetIgnoreCollisionWithHolder(bool ignore)
    {
        Collider cogCol = cog.GetComponent<MeshCollider>();
        foreach (var holderCol in _holder.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(cogCol, holderCol, ignore);
        }
    }

    private void UpdateAttachedPoseLocal()
    {
        // XZ만 스냅, Y는 현재 높이 유지
        Vector3 curr = _holderRb.position;
        Vector3 targetPos = new Vector3(_targetPos.x, curr.y, _targetPos.z);


        //_holder.transform.position = targetPos;
        _holderRb.MovePosition(targetPos);
        _holderRb.velocity = new Vector3(0f, _holderRb.velocity.y, 0f);

        // Yaw만 정렬
        Vector3 toCog = cog.transform.position - targetPos;
        toCog.y = 0f;
        if (toCog.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toCog.normalized, Vector3.up);
            _holderRb.MoveRotation(Quaternion.RotateTowards(_holderRb.rotation, targetRot, 720f * Time.fixedDeltaTime));
            _holderRb.angularVelocity = Vector3.zero; // 불필요한 회전 누름
        }
    }
    
    [PunRPC]
    private void RPC_GripAnchorTarget(int seq, Vector3 anchorPos)
    {
        if (seq <= _lastSeq) return; // 지연 패킷 무시
        _lastSeq = seq;

        _targetPos = anchorPos;
    }

    private void GripAnchorClear(int seq)
    {
        if (seq <= _lastSeq) return;
        _lastSeq = seq;
    }

    public void Release()
    {
        photonView.RPC(nameof(RPC_SetGrabState), RpcTarget.All, false, -1);
    }

    [PunRPC]
    private void RPC_SetGrabState(bool value, int characterViewId)
    {
        SetGrabState(value, characterViewId);
        cog.OnGripStateChange();
        if (!value)
        {
            GripAnchorClear(++_anchorSeq);
        }
    }

    private void SetGrabState(bool value, int characterViewId)
    {
        IsOccupied = value;
        HolderViewId = characterViewId;
        if (value)
        {
            HeldCharacterIds.Add(characterViewId);
            _holder = PhotonView.Find(characterViewId)?.GetComponent<CharacterBase>();
            SetIgnoreCollisionWithHolder(true);
            _holderRb = _holder?.GetComponent<Rigidbody>();
            //if (_col != null) _col.enabled = false;
            if (_holder != null && _holder.photonView.IsMine)
            {
                // 상호작용 주체라면 전체 grip 상호작용 비활성화
                OnGripStateChanged?.Invoke(false);
                LockLocalCharacter(_holder, true);
                EnableUI(true);
            }
            else
            {
                // 상호작용 주체가 아니라면 해당 grip만 상호작용 비활성화
                EnableInteraction(false);
            }
            _holder?.Anim.ResetDelta();
            _lastSeq = -1;
        }
        else
        {
            if (_holder == null) return;
            HeldCharacterIds.Remove(_holder.photonView.ViewID);
            SetIgnoreCollisionWithHolder(false);
            if (_holder != null && _holder.photonView.IsMine)
            {
                // 상호작용 주체라면
                OnGripStateChanged?.Invoke(true); // 전체 grip 상호작용 활성화
                LockLocalCharacter(_holder, false);
                EnableUI(false);
            }
            else
            {
                // 상호작용 주체가 아니라면
                EnableInteraction(true); // 해당 grip만 상호작용 활성화
            }
            _holder = null;
            _holderRb = null;
            //if (_col != null) _col.enabled = true;
        }
    }

    public void EnableInteraction(bool enable)
    {
        if (_col == null) return;
        _col.enabled = enable;
    }

    public void OnInteractAvailable() { }
    public void OnInteractUnavailable() { }
}