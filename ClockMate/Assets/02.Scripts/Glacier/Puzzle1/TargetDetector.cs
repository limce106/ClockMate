using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    private Camera _mainCam;
    private UIAim _uiAim;
    private List<ITurretTarget> _activeTargets;
    public ITurretTarget CurrentTarget { get; private set; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _mainCam = Camera.main;
        _activeTargets = new List<ITurretTarget>();
        _uiAim = UIManager.Instance.Show<UIAim>("UIAim");
    }

    private void Update()
    {
        DetectTarget();
    }

    private void DetectTarget()
    {
        CurrentTarget = null;

        // 조준 UI 화면 좌표 영역 계산
        Vector3[] corners = new Vector3[4];
        _uiAim.AimTransform.GetWorldCorners(corners);
        Vector3 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector3 topRight   = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        float xMin = Mathf.Min(bottomLeft.x, topRight.x);
        float xMax = Mathf.Max(bottomLeft.x, topRight.x);
        float yMin = Mathf.Min(bottomLeft.y, topRight.y);
        float yMax = Mathf.Max(bottomLeft.y, topRight.y);

        Rect aimRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

        Vector2 aimCenter = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);

        float minDistance = float.MaxValue;

        foreach (ITurretTarget target in _activeTargets)
        {
            if (target is not MonoBehaviour mbTarget) continue;
            
            Vector3 screenPos = _mainCam.WorldToScreenPoint(mbTarget.transform.position);
            if (screenPos.z < 0f || !aimRect.Contains(screenPos)) continue;
            
            float dist = Vector2.Distance(aimCenter, screenPos);
            if (dist < minDistance)
            {
                CurrentTarget = target;
                minDistance = dist;
            }
        }
        _uiAim.UpdateImage(CurrentTarget is not null);
    }
    
    public void AddTarget(ITurretTarget target)
    {
        _activeTargets.Add(target);
    }
    
    public void RemoveTarget(ITurretTarget target)
    {
        _activeTargets.Remove(target);
    }
}
