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
    [SerializeField] private GameObject activeInteractObj;
    
    private float _interactDistSqr;
    private float _cosAngle;
    private readonly List<GameObject> _tmpRemove = new();
    
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
        if (_detectedObjects.Count > 0) CleanupInvalidInteractables();

        _updateTimer += Time.deltaTime;
        if (_updateTimer >= UpdateInterval)
        {
            UpdateActiveInteractObj();
            _uiInteraction?.UpdateUIPosition();
            _updateTimer -= UpdateInterval;
        }
        activeInteractObj = _activeInteractObj;
    }
    
    private void Init()
    {
        _character = GetComponentInParent<CharacterBase>();
        _detectedObjects = new Dictionary<GameObject, IInteractable>();
        _interactDistSqr = interactDistance * interactDistance;
        _cosAngle = Mathf.Cos(interactAngle * Mathf.Deg2Rad);
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (_detectedObjects.ContainsKey(other.gameObject))
            RemoveDetectedObject(other.gameObject);
    }

    private void UpdateActiveInteractObj()
    {
        if (_detectedObjects.Count == 0) return;
        
        var charT = _character.transform;
        Vector3 charPos = charT.position; charPos.y = 0f;
        Vector3 forward = charT.forward; forward.y = 0f;
        forward.Normalize();

        float bestDistSqr = float.MaxValue;
        GameObject best = null;

        foreach (var pair in _detectedObjects)
        {
            GameObject targetObj = pair.Key;
            IInteractable interactable = pair.Value;
            if (targetObj == null) continue;

            // 거리 조건
            Vector3 targetObjPos = targetObj.transform.position;
            targetObjPos.y = 0;
            Vector3 dir = targetObjPos - charPos;
            float d2 = dir.sqrMagnitude;
            if (d2 > _interactDistSqr) continue;

            // 시야각 조건
            if(!ShouldIgnoreViewAngle(targetObj))
            {
                if (d2 > 1e-6f)
                {
                    float dot = Vector3.Dot(forward, dir / Mathf.Sqrt(d2));
                    if (dot < _cosAngle) continue;
                }
            }

            // 가장 가까운 오브젝트 && 상호작용 가능 여부
            if (d2 < bestDistSqr && interactable.CanInteract(_character))
            {
                bestDistSqr = d2;
                best = targetObj;
            }
        }

        // 선택된 오브젝트가 변경되었을 때만 Sprite 교체
        if (_activeInteractObj != best)
        {
            // 이전 선택된 오브젝트 UI Reset
            if (_activeInteractObj !=null)
            {
                _uiInteraction?.SetImage(_activeInteractObj, false);
                if (_detectedObjects.TryGetValue(_activeInteractObj, out var oldInter))
                    oldInter.OnInteractUnavailable();
            }

            // 새로 선택된 오브젝트 UI Set
            if (best != null)
            {
                _uiInteraction?.SetImage(best, true);
                if (_detectedObjects.TryGetValue(best, out var newInter))
                    newInter.OnInteractAvailable();
            }
            _activeInteractObj = best;
        }
    }

    private void CleanupInvalidInteractables()
    {
        _tmpRemove.Clear();
        foreach (var kv in _detectedObjects)
        {
            GameObject go = kv.Key;
            if (go == null || !go.activeInHierarchy)
            {
                _tmpRemove.Add(go);
                continue;
            }

            if (!go.TryGetComponent(out Collider col) || !col.enabled)
            {
                _tmpRemove.Add(go);
            }
        }

        foreach (GameObject go in _tmpRemove)
        {
            RemoveDetectedObject(go);
        }
    }

    private void RemoveDetectedObject(GameObject targetObj)
    {
        if (targetObj == null)
        {
            var dead = new List<GameObject>();
            foreach (var kv in _detectedObjects)
                if (kv.Key == null) dead.Add(kv.Key);

            foreach (var k in dead)
            {
                _detectedObjects.Remove(k);
                _uiInteraction?.DeactivateUI(k); // UI 매핑도 함께 제거
            }

            if (_activeInteractObj == null) return;
            _activeInteractObj = null;
            return;
        }

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
        if (_detectedObjects.TryGetValue(_activeInteractObj, out var inter))
        {
            if (inter.Interact(_character) && !inter.CanInteract(_character))
            {
                _uiInteraction?.DeactivateUI(_activeInteractObj);
                _activeInteractObj = null;
            }
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
