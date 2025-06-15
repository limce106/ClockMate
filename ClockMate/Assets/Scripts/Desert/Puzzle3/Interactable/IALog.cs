using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IALog : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 rotateAxis; // 회전 목표 EulerAngles 
    [SerializeField] private float moveSpeed; // 초당 회전 속도 (degrees/sec)
    [SerializeField] private float dropSpeed; 
    
    private Vector3 _startAxis;
    private bool _isInteracting;
    private CharacterBase _interactingCharacter;
    private UIManager _uiManager;
    private Coroutine _moveCoroutine;
    
    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        // E키를 그만 누르거나 시간이 끝나면 원 위치로 복귀
        if (_isInteracting && Input.GetKeyUp(KeyCode.E))
        {
            Drop();
        }
    }

    private void Init()
    {
        _startAxis = transform.rotation.eulerAngles;
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
        StartMove(character);
        
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
        
        // 플레이어 조작 비활성화
        _interactingCharacter.InputHandler.enabled = false;
        
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
        
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(Quaternion.Euler(rotateAxis)));
    }

    private void Drop()
    {
        _isInteracting = false;
        
        // 플레이어 조작 활성화
        _interactingCharacter.InputHandler.enabled = true;
        _interactingCharacter = null;
        
        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(Quaternion.Euler(_startAxis)));
    }

    private void OnDestroy()
    {
        if (_isInteracting)
        {
            //_uiManager.Close(_uiNotice);
        }
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
    }
    
    private IEnumerator MoveRoutine(Quaternion targetRotation)
    {
        float speed = _isInteracting ? moveSpeed : dropSpeed;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                speed * Time.deltaTime
            );

            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
