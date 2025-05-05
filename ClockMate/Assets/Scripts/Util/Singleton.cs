using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일반 싱글톤
/// </summary>
public abstract class Singleton<T> where T : Singleton<T>, new() // 기본 생성자 필요
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
}
