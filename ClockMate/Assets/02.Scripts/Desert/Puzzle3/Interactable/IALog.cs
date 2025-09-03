using System.Collections;
using System.Collections.Generic;
using DefineExtension;
using Photon.Pun;
using UnityEngine;

public class IALog : MonoBehaviourPun, IInteractable
{
    [SerializeField] private Vector3 rotateAxis; // 회전 목표 EulerAngles 
    [SerializeField] private float moveSpeed; // 초당 회전 속도 (degrees/sec)
    [SerializeField] private float dropSpeed; 
    
    private Vector3 _startAxis;
    private bool _isInteracting;
    private CharacterBase _interactingCharacter;
    private UIManager _uiManager;
    private Coroutine _moveCoroutine;
    private Quaternion _startLocalRot;
    private Vector3 _startLocalEuler;
    
    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        // E키를 그만 누르거나 시간이 끝나면 원 위치로 복귀
        if (_isInteracting && Input.GetKeyUp(KeyCode.E))
        {
            NetworkExtension.RunNetworkOrLocal(
                Drop,
                () => photonView.RPC(nameof(RPC_DropLog), RpcTarget.All)
            );
        }
    }

    private void Init()
    {
        _startLocalRot   = transform.localRotation;
        _startLocalEuler = transform.localEulerAngles;
        
        _isInteracting = false;
        _uiManager = UIManager.Instance;
    }
    
    public bool CanInteract(CharacterBase character)
    {
        return !_isInteracting && character is Hour;
    }

    public void OnInteractAvailable()
    {
        
    }

    public void OnInteractUnavailable()
    {
        
    }

    public bool Interact(CharacterBase character)
    {
        // 들어올리기
        NetworkExtension.RunNetworkOrLocal(
            () => StartMove(character),
            () => photonView.RPC(nameof(RPC_MoveLog), RpcTarget.All, character.photonView.ViewID)
        );
        
        // 상호작용 중 UI 활성화
        // _uiNotice = _uiManager.Show<UINotice>("UINotice");
        // _uiNotice.SetImage(_dropSprite);
        // _uiNotice.SetText(_dropString);
        
        return true;
    }

    private void StartMove(CharacterBase character)
    {
        _isInteracting = true;
        _interactingCharacter = character;
        
        if (_interactingCharacter.photonView.IsMine)
        {
            // 플레이어의 캐릭터라면 조작 비활성화
            _interactingCharacter.InputHandler.enabled = false;
        }

        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
        
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        Vector3 targetLocalEuler = new Vector3(rotateAxis.x, rotateAxis.y, rotateAxis.z);
        _moveCoroutine = StartCoroutine(MoveRoutine(Quaternion.Euler(targetLocalEuler)));
    }

    private void Drop()
    {
        _isInteracting = false;
        
        if (_interactingCharacter.photonView.IsMine)
        {
            // 플레이어의 캐릭터라면 조작 활성화
            _interactingCharacter.InputHandler.enabled = true;
        }
        _interactingCharacter = null;
        
        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(_startLocalRot));
    }

    private void OnDestroy()
    {
        if (_isInteracting)
        {
            //_uiManager.Close(_uiNotice);
        }
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
    }
    
    private IEnumerator MoveRoutine(Quaternion targetLocalRotation)
    {
        float speed = _isInteracting ? moveSpeed : dropSpeed;
        while (Quaternion.Angle(transform.localRotation, targetLocalRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, targetLocalRotation, speed * Time.deltaTime);
            yield return null;
        }
        transform.localRotation = targetLocalRotation;
    }

    [PunRPC]
    public void RPC_MoveLog(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null) return;

        CharacterBase character = view.GetComponent<CharacterBase>();
        if (character == null) return;
        
        StartMove(character);
    } 
    
    [PunRPC]
    public void RPC_DropLog()
    {
        Drop();
    }
}
