using Photon.Pun;
using UnityEngine;

public abstract class ResettableBase : MonoBehaviourPun
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
	/// 초기 상태 저장하는 로직을 상속받은 자식 블럭 클래스에서 각자 구현
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
	/// 초기화 로직을 상속받은 자식 블럭 클래스에서 각자 구현
	/// </summary>
	public abstract void ResetObject();

	[PunRPC]
    public void RPC_ResetObject()
    {
        ResetObject();
    }
}