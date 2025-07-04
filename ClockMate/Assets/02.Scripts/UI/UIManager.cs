using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 전체 UI를 통합 관리하는 매니저.
/// UI 프리팹 로드, 표시, 캐시함.
/// 최상단 UI 관리 및 esc 키 처리 기능 지원
/// </summary>
public class UIManager : MonoSingleton<UIManager>
{
    [field: SerializeField] public CanvasScaler CommonScaler { get; private set; }
    [SerializeField] RectTransform rtSafeArea; // 실제 UI 붙는 공간
    [SerializeField] RectTransform rtCachedArea; // 비활성화된 UI 캐시되는 공간

    private Dictionary<string, UIBase> _cachedDict; // 로드된 UI 프리팹 캐시
    private List<UIBase> _uiList; // 현재 화면 표시되는 UI 리스트

    private UIBase Peek => _uiList.Count == 0 ? null : _uiList[^1]; // 최상단 UI 가져오기

    protected override void Init()
    {
        base.Init();
        _cachedDict = new Dictionary<string, UIBase>();
        _uiList = new List<UIBase>();
    }

    /// <summary>
    /// UI를 표시한다. (캐시되어 있으면 재활용, 없으면 Resources에서 로드)
    /// </summary>
    public T Show<T>(string address) where T : UIBase
    {
        T uiBase;
        
        if (_cachedDict.TryGetValue(address, out UIBase value))
        {
            uiBase = value as T;
            uiBase.transform.SetParent(rtSafeArea, true);
            uiBase.gameObject.SetActive(true);
        }
        else
        {
            var prefab = Resources.Load<GameObject>("UI/" + address);
            uiBase = Instantiate(prefab, rtSafeArea).GetComponent<T>();
            _cachedDict[address] = uiBase;
        }
        
        // 공통코드: FullScreen UI라면 아래 UI는 비활성화
        if (_uiList.Count > 0 && uiBase.UIType == UI.UIType.FullScreen)
        {
            foreach (UIBase ui in _uiList)
            {
                ui.gameObject.SetActive(false);
            }
        }
        
        uiBase.Show();
        _uiList.Add(uiBase);
        return uiBase;
    }

    /// <summary>
    /// UI를 닫는다. (스택 최상단 UI만 닫을 수 있음)
    /// </summary>
    public bool Close(UIBase targetUi)
    {
        // 최상단에 있는 UI가 아니라면 닫지 않는다.
        if (targetUi != Peek)
        {
            Debug.LogError($"잘못된 UI를 닫으려고 함. {targetUi.name}");
            return false;
        }

        _uiList.RemoveAt(_uiList.Count - 1);
        targetUi.gameObject.SetActive(false);
        targetUi.transform.SetParent(rtCachedArea, true);

        // FullScreen UI 켤 때 비활성화 했던 UI들 복구
        if (targetUi.UIType == UI.UIType.FullScreen)
        {
            for (int i = _uiList.Count - 1; i >= 0; --i)
            {
                UIBase ui = _uiList[i];
                ui.gameObject.SetActive(true);
                if (ui.UIType == UI.UIType.FullScreen) break;
            }
        }

        return true;
    }

    private void Update()
    {
        // Esc 키 입력 시 최상단 UI의 BackKey 호출
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_uiList.Count == 0) return;
            
            if (Peek.BackKey())
            {
                Peek.Close();
            }
        }
    }
}