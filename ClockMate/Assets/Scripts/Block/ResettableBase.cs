using UnityEngine;

public abstract class ResettableBase : MonoBehaviour
{
	protected virtual void Awake()
	{
		Init();
		SaveInitialState();
		Register();
	}
	
	protected virtual void Init()
	{
		
	}

	/// <summary>
	/// 초기 상태 저장
	/// </summary>
	protected abstract void SaveInitialState();

	/// <summary>
	/// 리셋 매니저에 오브젝트 등록
	/// </summary>
	protected virtual void Register()
	{
		ResetTestManager.Instance.AddResettable(this);
	}
	
	/// <summary>
	/// 초기화 로직
	/// </summary>
	public abstract void ResetObject();
}