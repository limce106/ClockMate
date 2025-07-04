public interface IInteractable
{
    /// <summary>
    /// 매개변수로 받은 캐릭터와의 상호작용 가능 여부
    /// </summary>
    bool CanInteract(CharacterBase character);
    /// <summary>
    /// 상호작용 가능해진 시점에 한 번 동작
    /// </summary>
    void OnInteractAvailable();
    /// <summary>
    /// 상호작용 불가능해진 시점에 한 번 동작
    /// </summary>
    void OnInteractUnavailable();
    /// <summary>
    /// 상호작용 시 동작할 로직
    /// </summary>
    bool Interact(CharacterBase character);
}
