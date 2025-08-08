using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝트 감지 및 처리
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactDistance = 5.0f; // 상호작용 가능 거리
    [SerializeField] private float interactAngle = 60.0f; // 상호작용 가능 시야각
    [SerializeField] private GameObject activeInteractObj; // 상호작용 가능 시야각
    
    private CharacterBase _character;
    private Dictionary<GameObject, IInteractable> _detectedObjects;
    private GameObject _activeInteractObj;
    private UIInteraction _uiInteraction;
    
    private float _updateTimer = 0f;
    private const float UpdateInterval = 0.1f; // ui 갱신 주기

    private void Awake()
    {
        Init();
        if (!_character.photonView.IsMine)
        {
            // 내 캐릭터가 아니라면 상호작용 감지 비활성화
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= UpdateInterval)
        {
            UpdateActiveInteractObj();
            _updateTimer -= UpdateInterval;
        }
        _uiInteraction?.UpdateUIPosition();

        activeInteractObj = _activeInteractObj;
    }
    
    private void Init()
    {
        _character = GetComponentInParent<CharacterBase>();
        _detectedObjects = new Dictionary<GameObject, IInteractable>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable)) return;

        // 상호작용 가능한 Interactable이 감지되었다면

        _detectedObjects.TryAdd(other.gameObject, interactable);
        if (_uiInteraction == null)
        {
            _uiInteraction = UIManager.Instance.Show<UIInteraction>("UIInteractionRoot");
        }
        _uiInteraction.ActivateUI(other.gameObject);
        // TODO 게임 오브젝트가 제거되었을 경우 처리 추가
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable)) return;
        
        RemoveDetectedObject(other.gameObject);
    }

    private void UpdateActiveInteractObj()
    {
        if (_detectedObjects.Count == 0) return;
        
        float closestDistance = float.MaxValue;
        GameObject availableObj = null;

        foreach (var pair in _detectedObjects)
        {
            GameObject targetObj = pair.Key;
            IInteractable interactable = pair.Value;
            
            if (targetObj == null) continue;

            // 거리 조건
            Vector3 charPos = _character.transform.position;
            Vector3 targetObjPos = targetObj.transform.position;

            charPos.y = 0;
            targetObjPos.y = 0;

            float distance = Vector3.Distance(charPos, targetObjPos);
            if (distance > interactDistance) continue;

            // 시야각 조건
            if(!ShouldIgnoreViewAngle(targetObj))
            {
                Vector3 direction = (targetObjPos - charPos).normalized;
                Vector3 forward = _character.transform.forward;
                forward.y = 0;
                float angle = Vector3.Angle(_character.transform.forward, direction);
                if (angle > interactAngle) continue;
            }

            // 가장 가까운 오브젝트 && 상호작용 가능 여부
            if (distance < closestDistance && interactable.CanInteract(_character))
            {
                closestDistance = distance;
                availableObj = targetObj;
            }
        }

        // 선택된 오브젝트가 변경되었을 때만 Sprite 교체
        if (_activeInteractObj != availableObj)
        {
            // 이전 선택된 오브젝트 UI Reset
            if (_activeInteractObj is not null)
            {
                _uiInteraction?.SetImage(_activeInteractObj, false);
                _detectedObjects[_activeInteractObj]?.OnInteractUnavailable();
            }

            // 새로 선택된 오브젝트 UI Set
            if (availableObj is not null)
            {
                _uiInteraction?.SetImage(availableObj, true);
                _detectedObjects[availableObj]?.OnInteractAvailable();
            }

            _activeInteractObj = availableObj;
        }
    }
    
    private void RemoveDetectedObject(GameObject targetObj)
    {
        // dictionary에서 제거, UI 비활성화 처리
        _detectedObjects.Remove(targetObj);
        _uiInteraction?.DeactivateUI(targetObj);
        if (targetObj == _activeInteractObj)
        {
            _activeInteractObj = null;
        }
    }

    public void TryInteract()
    {
        if (_activeInteractObj == null) return;
        IInteractable interactable = _detectedObjects[_activeInteractObj];
        if (interactable.Interact(_character) && !interactable.CanInteract(_character))
        {
            _uiInteraction?.DeactivateUI(_activeInteractObj);
            _activeInteractObj = null;
        }
    }

    /// <summary>
    /// 시야각 예외 체크 함수
    /// </summary>
    private bool ShouldIgnoreViewAngle(GameObject obj)
    {
        if(obj.TryGetComponent<TreeClimbObject>(out var tree))
        {
            return _character.transform.position.y >= obj.transform.position.y;
        }

        return false;
    }
}
