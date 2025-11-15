using UnityEngine;

public class DoorConditionController : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private bool lockAfterEntry;
    [SerializeField] private bool openWithOnePress;

    private IDoorCondition _openCondition;

    private void Awake()
    {
        TryGetComponent(out _openCondition);
    }

    private void Update()
    {
        if (door.IsLocked) return;

        if (!_openCondition.IsConditionMet() && !openWithOnePress)
        {
            door.Close();
            return;
        }

        if (_openCondition.IsConditionMet())
        {
            door.Open();
        }
    }

    public void EnterDoor()
    {
        if (!door.IsLocked && lockAfterEntry)
        {
            door.Lock();
        }
    }
}
