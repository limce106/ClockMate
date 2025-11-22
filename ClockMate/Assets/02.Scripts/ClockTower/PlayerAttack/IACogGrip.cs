using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PhotonView))]
public class IACogGrip : MonoBehaviourPun, IInteractable
{
    public enum GripSide { A, B }

    [Header("Binding")]
    public Cog cog;
    public GripSide side;

    private Collider _gripCol;
    public bool IsOccupied { get; private set; }
    public int HolderViewId { get; private set; }
    
    private CharacterBase _holder;
    private Rigidbody _holderRb;
    private Collider _holderCol;
    
    private UINotice _uiNotice;
    private Sprite _dropSprite;
    private string _dropString;

    private static readonly HashSet<int> HeldCharacterIds = new ();
    public Action<bool> OnGripStateChanged;
    
    private bool _isApproaching;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _gripCol = GetComponent<Collider>();
        if (_gripCol == null)
        {
            _gripCol = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)_gripCol).radius = 0.3f;
            _gripCol.isTrigger = true;
        }

        IsOccupied = false;
        HolderViewId = -1;
        _dropSprite = Icon.Key.Q.LoadSprite(Icon.Style.Outline);;
        _dropString = "내려놓기";
    }

    private void Update()
    {
        // 점유 상태이고, 내 캐릭터가 잡고 있다면 입력 체크
        if (!IsOccupied || _holder == null || !_holder.photonView.IsMine) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Release();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOccupied) return;

        // 내 캐릭터가 잡고 있다면 로컬에서 위치 고정 수행
        if (_holder != null && _holder.photonView.IsMine)
        {
            UpdateAttachedPoseLocal();
        }
    }

    public bool CanInteract(CharacterBase character)
    {
        if (!cog || !character) return false;
        if (_isApproaching) return false; // 이미 접근 중이면 중복 실행 방지
        // 플레이어가 다른 grip을 잡고있지 않고 해당 grip을 상대가 차지하지 않아야 true
        return !HeldCharacterIds.Contains(character.photonView.ViewID) && !IsOccupied;
    }

    public bool Interact(CharacterBase character)
    {
        //로컬에서 먼저 Approach 시퀀스 시작
        if (character.photonView.IsMine)
        {
            StartCoroutine(MoveToGripRoutine(character));
        }
        return true;
    }

    /// <summary>
    /// 캐릭터가 그립 위치로 걸어가 회전한 뒤 잡도록 하는 코루틴
    /// </summary>
    private IEnumerator MoveToGripRoutine(CharacterBase character)
    {
        _isApproaching = true;
        character.InputHandler.enabled = false;
        
        // 충돌 무시
        Collider charCol = character.GetComponent<Collider>();
        if(charCol != null) cog.SetIgnoreCollision(charCol, true);

        Rigidbody rb = character.GetComponent<Rigidbody>();
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); 

        NavMeshPath path = new NavMeshPath();
        // y값은 캐릭터 발바닥 높이(현재 높이)로 보정하여 계산
        Vector3 targetPos = new Vector3(transform.position.x, character.transform.position.y, transform.position.z);
        
        if (NavMesh.CalculatePath(character.transform.position, targetPos, NavMesh.AllAreas, path))
        {
            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 nextCorner = path.corners[i];
                while (true)
                {
                    Vector3 dir = nextCorner - character.transform.position;
                    dir.y = 0; // 수평 이동 강제
                    float dist = dir.magnitude;
                    float stopDist = (i == path.corners.Length - 1) ? 0.1f : 0.5f;

                    if (dist <= stopDist) break;

                    character.Move(dir.normalized);

                    // 이동 중에 톱니바퀴를 밟고 붕 뜨는 현상을 막기 위해 하단으로 약간의 힘을 지속적으로 줌 (중력 보조)
                    if (!character.IsGrounded) // CharacterBase에 IsGrounded가 있다고 가정
                    {
                        rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
                    }

                    yield return new WaitForFixedUpdate();
                }
            }
            // 도착 성공 처리
            rb.velocity = Vector3.zero;
            character.transform.position = new Vector3(transform.position.x, character.transform.position.y, transform.position.z);
            
            Vector3 toCog = cog.transform.position - character.transform.position;
            toCog.y = 0;
            if (toCog.sqrMagnitude > 0.001f)
                character.transform.rotation = Quaternion.LookRotation(toCog);

            photonView.RPC(nameof(RPC_SetGrabState), RpcTarget.All, true, character.photonView.ViewID);
        }
        else
        {
            // 실패 처리
            Debug.LogWarning("[IACogGrip] 경로 계산 실패, 플레이어 움직임 재활성");
            character.InputHandler.enabled = true;
            cog.SetIgnoreCollision(charCol, false);
        }
        
        _isApproaching = false;
    }
    
    private void EnableUI(bool enable)
    {
        if (enable)
        {
            _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
            _uiNotice.SetImage(_dropSprite);
            _uiNotice.SetText(_dropString);
            _uiNotice.SetVerticalPos(false);
        } else if (_uiNotice != null)
        {
            _uiNotice.Close();
            _uiNotice = null;
        }
        
    }

    private void UpdateAttachedPoseLocal()
    {
        Vector3 gripPos = transform.position; // 그립의 월드 위치
        Vector3 currPos = _holderRb.position; // 캐릭터의 현재 위치

        Vector3 targetPos = new Vector3(gripPos.x, currPos.y, gripPos.z); 

        // 물리 이동
        _holderRb.MovePosition(targetPos);
        _holderRb.velocity = Vector3.zero;

        // 회전 처리
        Vector3 toCog = cog.transform.position - targetPos;
        toCog.y = 0f;
        if (toCog.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toCog.normalized, Vector3.up);
            _holderRb.MoveRotation(Quaternion.RotateTowards(_holderRb.rotation, targetRot, 720f * Time.fixedDeltaTime));
        }
    }
    
    public void Release()
    {
        if (_isApproaching) return; // 접근 중 취소 방지
        photonView.RPC(nameof(RPC_SetGrabState), RpcTarget.All, false, -1);
    }

    [PunRPC]
    private void RPC_SetGrabState(bool value, int characterViewId)
    {
        SetGrabState(value, characterViewId);
        cog.OnGripStateChange();
    }

    private void SetGrabState(bool value, int characterViewId)
    {
        IsOccupied = value;
        HolderViewId = characterViewId;
        if (value) // 잡았을 때
        {
            HeldCharacterIds.Add(characterViewId);
            _holder = PhotonView.Find(characterViewId)?.GetComponent<CharacterBase>();
            _holderRb = _holder?.GetComponent<Rigidbody>();
            _holderCol = _holder?.GetComponent<Collider>();

            // 잡은 상태에서는 충돌 무시 유지
            if(_holderCol != null) cog.SetIgnoreCollision(_holderCol, true);

            if (_holder != null && _holder.photonView.IsMine)
            {
                OnGripStateChanged?.Invoke(false);
                EnableUI(true);
            }
            else
            {
                EnableInteraction(false);
            }
            
            // 애니메이션 리셋
            _holder?.Anim.ResetDelta();
        }
        else // 놓았을 때
        {
            if (_holder == null) return;
            
            // 충돌 무시 해제
            if(_holderCol != null) cog.SetIgnoreCollision(_holderCol, false);

            HeldCharacterIds.Remove(_holder.photonView.ViewID);

            if (_holder != null && _holder.photonView.IsMine)
            {
                OnGripStateChanged?.Invoke(true);
                _holder.InputHandler.enabled = true; // 입력 잠금 해제
                EnableUI(false);
            }
            else
            {
                EnableInteraction(true);
            }
            
            _holder = null;
            _holderRb = null;
            _holderCol = null;
        }
    }

    public void EnableInteraction(bool enable)
    {
        if (_gripCol == null) return;
        _gripCol.enabled = enable;
    }

    public void OnInteractAvailable() { }
    public void OnInteractUnavailable() { }
}