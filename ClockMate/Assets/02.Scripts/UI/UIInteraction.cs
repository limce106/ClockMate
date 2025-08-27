using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define.UI.UIType;

public class UIInteraction : UIBase
{
    [SerializeField] private Image imgInteractionPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private List<Image> _uiImagePool;
    private List<GameObject> _tmpRemove;
    private Dictionary<GameObject, Image> _objectToUIImage;
    private Sprite _spriteInactive;
    private Sprite _spriteActive;
    private Camera _mainCamera;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        UIType = Windowed;
        _uiImagePool = new List<Image>(initialPoolSize);
        _tmpRemove = new List<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            _uiImagePool.Add(Instantiate(imgInteractionPrefab, transform));
            _uiImagePool[i].gameObject.SetActive(false);
        }
        _objectToUIImage = new Dictionary<GameObject, Image>();
        _spriteInactive = Resources.Load<Sprite>("UI/Sprites/interact_inactive");
        _spriteActive = Resources.Load<Sprite>("UI/Sprites/interact_active");
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// interactable UI 활성화.
    /// </summary>
    public void ActivateUI(GameObject detectedObj)
    {
        if (_objectToUIImage.ContainsKey(detectedObj)) return;
        Image image = GetImageFromPool();
        image.sprite = _spriteInactive; // 기본값으로 초기화
        _objectToUIImage[detectedObj] = image;
    }

    /// <summary>
    /// interactable UI 비활성화. 
    /// </summary>
    public void DeactivateUI(GameObject detectedObj)
    {
        if (!_objectToUIImage.ContainsKey(detectedObj)) return;
        ReturnImageToPool(_objectToUIImage[detectedObj]);
        _objectToUIImage.Remove(detectedObj);        
    }

    public void UpdateUIPosition()
    {
        if (_objectToUIImage.Count == 0) return;
        
        _tmpRemove.Clear();
        foreach (var pair in _objectToUIImage)
        {
            if (pair.Key == null) { _tmpRemove.Add(pair.Key); }
        }
        
        foreach (var k in _tmpRemove)
        {
            if (_objectToUIImage.TryGetValue(k, out var img)) ReturnImageToPool(img);
            _objectToUIImage.Remove(k);
        }
        if (_objectToUIImage.Count == 0) return;
        
        foreach (var pair in _objectToUIImage)
        {
            GameObject obj = pair.Key; 
            Image uiImage = pair.Value;
            Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(obj.transform.position);

            if (viewportPoint.z <= 0)
            {
                // 카메라 뒤에 있는 오브젝트는 UI 비활성화
                uiImage.gameObject.SetActive(false);
                continue;
            }

            bool inView = viewportPoint.x is >= 0f and <= 1f && viewportPoint.y is >= 0f and <= 1f;
            Vector3 sp;
            if (inView)
                sp = _mainCamera.WorldToScreenPoint(obj.transform.position);
            else
                sp = CalculateEdgePosition(viewportPoint);

            sp.x = Mathf.Round(sp.x); sp.y = Mathf.Round(sp.y);
            uiImage.transform.position = sp;
            if (!uiImage.gameObject.activeSelf) uiImage.gameObject.SetActive(true);
            // if (IsInView(pair.Key))
            // {
            //     Vector3 screenPoint = _mainCamera.WorldToScreenPoint(pair.Key.transform.position);
            //     screenPoint.x = Mathf.Round(screenPoint.x);
            //     screenPoint.y = Mathf.Round(screenPoint.y);
            //     uiImage.transform.position = screenPoint;
            // }
            // else
            // {
            //     Vector3 edgeScreenPoint = CalculateEdgePosition(viewportPoint);
            //     edgeScreenPoint.x = Mathf.Round(edgeScreenPoint.x);
            //     edgeScreenPoint.y = Mathf.Round(edgeScreenPoint.y);
            //     uiImage.transform.position = edgeScreenPoint;
            // }
        }
    }

    /// <summary>
    /// Viewport 좌표를 받아 화면 가장자리 UI 위치 계산
    /// </summary>
    private Vector3 CalculateEdgePosition(Vector3 viewportPoint)
    {
        Vector3 clampedPoint = viewportPoint;

        // x가 왼쪽(0보다 작음) 또는 오른쪽(1보다 큼)
        if (viewportPoint.x < 0) clampedPoint.x = 0.05f;
        else if (viewportPoint.x > 1) clampedPoint.x = 0.95f;

        // y가 아래(0보다 작음) 또는 위(1보다 큼)
        if (viewportPoint.y < 0) clampedPoint.y = 0.05f;
        else if (viewportPoint.y > 1) clampedPoint.y = 0.95f;

        // z는 무조건 양수로 보정
        clampedPoint.z = Mathf.Max(clampedPoint.z, 0.1f);

        // Viewport -> Screen 좌표 변환
        return _mainCamera.ViewportToScreenPoint(clampedPoint);
    }
    
    /// <summary>
    /// 상호작용 가능 오브젝트로 선택되었는지 여부에 따라 UI 이미지 교체
    /// </summary>
    public void SetImage(GameObject detectedObj, bool isSelected)
    {
        if (!_objectToUIImage.TryGetValue(detectedObj, out Image img)) return;
        
        if ((isSelected && img.sprite != _spriteActive) ||
            (!isSelected && img.sprite != _spriteInactive))
        {
            img.sprite = isSelected ? _spriteActive : _spriteInactive;
        }
    }

    /// <summary>
    /// 화면 내에 보이는지 여부를 반환
    /// </summary>
    private bool IsInView(GameObject targetObj)
    {
        Vector3 screenPoint = _mainCamera.WorldToViewportPoint(targetObj.transform.position);
        return screenPoint is { z: > 0, x: >= 0 and <= 1, y: >= 0 and <= 1 };
    }
    
    private Image GetImageFromPool()
    {
        if (_uiImagePool.Count == 0)
        {
            Debug.LogError("UIInteraction Pool이 부족하여 확장");
            for (int i = 0; i < initialPoolSize; i++)
            {
                _uiImagePool.Add(Instantiate(imgInteractionPrefab, transform));
                _uiImagePool[i].gameObject.SetActive(false);
            }
        }
        Image img = _uiImagePool[^1];
        _uiImagePool.Remove(img);
        img.gameObject.SetActive(true);
        return img;
    }
    private void ReturnImageToPool(Image img)
    {
        _uiImagePool.Add(img);
        img.gameObject.SetActive(false);
    }
}
