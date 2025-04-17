using UnityEngine;

/// <summary>
/// MonoBehaviour 기반 싱글톤
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static bool IsInit { get; private set; } = false; // 초기화 여부
    
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에 인스턴스 존재하면 사용
                _instance = FindObjectOfType<T>();
                
                // 없으면 새로 생성
                if (_instance == null)
                {
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (IsInit)
        {
            Destroy(gameObject); // 중복 제거
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        IsInit = true;
        Init();
    }

    /// <summary>
    /// 인스턴스가 생성된 후 1회 호출되는 초기화 함수
    /// </summary>
    protected virtual void Init()
    {
        
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
